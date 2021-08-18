using System;
using _Scripts.Unit.AI;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.Player
{
    public class RollAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private ParticleSystem _rollEffect;

        [SerializeField] private int _rollMaxTime;
        
        [SerializeField] private float _rollRegenerateTime;
        private float _regenerateTime;
        public delegate void TimeChange(float time);
        public event TimeChange TimeChanged;
        
        private int _rollingTime;
        public int RollingTime
        {
            get => _rollingTime;
            private set
            {
                _rollingTime = value;
                ChargeChanged?.Invoke(value);
            }
        }
        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;
        
        [SerializeField] private double _rollTimeToStart;
        private float _rollTimer;
        
        public bool InRoll { get; private set; }
        
        [Space]

        [SerializeField] private float _timeBetweenAttack;
        [SerializeField] private float _rollConsumptionTime;
        
        [SerializeField] private float _rollDamage;
        [SerializeField] private float _rollRadius;
        [SerializeField] private float _knockback;
        
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _hit;

        [SerializeField] private AudioSource _rollAudioSource;

        [SerializeField] private AudioSource _jagAudioSource;
        [SerializeField] private AudioClip _jaggernaut;
        private float _rotation;
        
        private Unit _unit;
        private PhotonView _photonView;
        private readonly Collider[] _results = new Collider[10];
        [SerializeField] private ParticleSystem _hitEffect;
        
        private readonly CompositeDisposable _effectHit = new CompositeDisposable();

        private void Awake()
        {
            _effectHit.AddTo(this);
        }
        
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();

            RollingTime = _rollMaxTime;
            var _transform = transform;
            
            if (!_photonView.IsMine) return;
            
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (RollingTime < _rollMaxTime)
                {
                    _regenerateTime += (1 / _rollRegenerateTime) * Time.deltaTime;

                    var normalized = _regenerateTime;
                    TimeChanged?.Invoke(normalized);
                    
                    if (normalized >= 1f)
                    {
                        _regenerateTime = 0;

                        RollingTime++;
                    }
                }
            }).AddTo(this);
            
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (!InRoll) StartRoll();
                else Rolling();
                
                if (_rollAudioSource.time >= 1f)
                {
                    _rollAudioSource.time = .2f;
                }
            }).AddTo(this);
            
            Observable.Interval(TimeSpan.FromSeconds(_timeBetweenAttack)).Subscribe(x =>
            {
                if (!InRoll) return;
                
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _rollRadius, _results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    var unit = _results[i].GetComponent<Unit>();
                    if (unit != null && unit.enabled)
                    {
                        var position = unit.transform.position;
                        var posTo = (position - _transform.position).normalized;

                        _photonView.RPC(nameof(HitEffect), RpcTarget.AllViaServer,
                            position.x, position.y, position.z, Random.Range(0, 100));
                        
                        unit.PhotonView.RPC(nameof(AIHealth.TakeDamage), RpcTarget.AllViaServer,
                            _rollDamage, _unit.BounceDamage, _unit.TimeToStan);
                        
                        posTo.y = 0;
                        posTo *= _knockback;
                        unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                            posTo.x, posTo.y, posTo.z);
                    }
                }
            }).AddTo(this);
            
            Observable.Interval(TimeSpan.FromSeconds(_rollConsumptionTime)).Subscribe(x =>
            {
                if (InRoll)
                {
                    if (RollingTime > 0)
                        RollingTime--;
                    else
                        StopRolling();
                }
            }).AddTo(this);
        }

        [PunRPC]
        private void HitEffect(float x, float y, float z, int seed)
        {
            _audioSource.PlayOneShot(_hit[new System.Random(seed).Next(0, _hit.Length)]);
            var p = Instantiate(_hitEffect);
            p.transform.position = new Vector3(x, y + .5f, z);
            
            _effectHit.Add(Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(c =>
            {
                Destroy(p.gameObject);
            }));
        }
        
        private void Rolling()
        {
            if (!Input.GetKey(KeyCode.Mouse0)) StopRolling();
        }

        private void StartRoll()
        {
            if (_rollTimer >= _rollTimeToStart)
            {
                if(_unit.CurrentState == UnitState.Default) StartRolling();
                else _rollTimer = 0;
            }
            else
            {
                if (Input.GetKey(KeyCode.Mouse0)) _rollTimer += Time.deltaTime;
                else _rollTimer = 0;
            }
        }
        
        private void StopRolling()
        {
            InRoll = false;
            _rollTimer = 0;
            _effectHit.Clear();
            _photonView.RPC(nameof(Effect), RpcTarget.AllViaServer, false, Random.Range(0, 100));
        }
        
        private void StartRolling()
        {
            InRoll = true;
            _photonView.RPC(nameof(Effect), RpcTarget.AllViaServer, true, Random.Range(0, 100));
        }

        [PunRPC]
        private void Effect(bool start, int seed)
        {
            if (start)
            {
                _rollEffect.Play();

                _rollAudioSource.Play();
                if (new System.Random(seed).Next(0, 4) == 0) _jagAudioSource.PlayOneShot(_jaggernaut);
            }
            else
            {
                _rollAudioSource.Stop();
                _rollEffect.Stop();
            }
        }
    }
}
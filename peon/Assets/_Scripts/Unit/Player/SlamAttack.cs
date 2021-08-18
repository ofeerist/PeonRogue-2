using System;
using _Scripts.Unit.AI;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    public class SlamAttack : MonoBehaviour
    {
        [SerializeField] private Transform _attackPosition;

        [Space]

        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _attackRadius;
        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;

        [Space]

        [SerializeField] private int _maxCharges;
        
        [SerializeField] private float _chargeRegenerateTime;
        private float _regenerateTime;
        
        public delegate void TimeChange(float time);
        public event TimeChange TimeChanged;
        
        private int _currentCharges;
        public int CurrentCharges
        {
            get => _currentCharges;
            private set
            {
                _currentCharges = value;
                ChargeChanged?.Invoke(value);
            }
        }

        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;

        [Space]

        [SerializeField] private float _inAttackDuration;

        [Space]

        [SerializeField] private ParticleSystem _slam;
        [SerializeField] private float _slamDelay;

        private Unit _unit;
        private Animator _animator;
        private PhotonView _photonView;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clap;
        
        private static readonly int AttackNum = Animator.StringToHash("AttackNum");
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        private readonly Collider[] _results = new Collider[10];
        
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private readonly SerialDisposable _effectDisposable = new SerialDisposable();
        [SerializeField] private LayerMask _layerMask;
        private Movement _movement;

        private void Awake()
        {
            _serialDisposable.AddTo(this);
            _effectDisposable.AddTo(this);
        }
        
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _movement = GetComponent<Movement>();
            
            _animator = _unit.Animator;
            _photonView = _unit.PhotonView;

            _attackCooldownTimer = Time.time;

            if (!_photonView.IsMine) return;
            
            CurrentCharges = _maxCharges;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (CurrentCharges < _maxCharges)
                {
                    _regenerateTime += (1 / _chargeRegenerateTime) * Time.deltaTime;

                    var normalized = _regenerateTime;
                    TimeChanged?.Invoke(normalized);
                    
                    if (normalized >= 1f)
                    {
                        _regenerateTime = 0;

                        CurrentCharges++;
                    }
                }
            }).AddTo(this);

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Q) && _unit.CurrentState == UnitState.Default &&
                            CurrentCharges > 0 && _attackCooldownTimer < Time.time)
                .Subscribe(x =>
                {
                    _unit.CurrentState = UnitState.Attack;
                    
                    var ray = _unit.Camera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        _movement.LookPosition = hit.point;
                        _movement.LookPosition.y = 0;
                    }

                    _attackCooldownTimer = _attackCooldown + Time.time;
                    CurrentCharges -= 1;

                    _animator.SetInteger(AttackNum, 2);
                    _animator.SetTrigger(Attack1);
                    
                    _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_slamDelay)).Subscribe(z =>
                    {
                        var position1 = _attackPosition.position;
                        var position = position1;
                        _unit.PhotonView.RPC(nameof(PlayEffect), RpcTarget.All, position.x,
                            position.y, position.z);

                        
                        var size = Physics.OverlapSphereNonAlloc(position1, _attackRadius, _results, _layerMask);
                        for (int i = 0; i < size; i++)
                        {
                            var unit = _results[i].GetComponent<Unit>();
                            var posTo = (_results[i].transform.position - transform.position).normalized;
                            if (unit != null && unit.CurrentState != UnitState.Dead)
                            {
                                unit.PhotonView.RPC(nameof(AIHealth.TakeDamage), RpcTarget.AllViaServer,
                                    _damage, _unit.BounceDamage, _unit.TimeToStan);

                                posTo.y = 0;
                                posTo *= _knockback;
                                unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                                    posTo.x, posTo.y, posTo.z);
                            }
                        }

                        _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_inAttackDuration))
                            .Subscribe(c =>
                            {
                                _unit.CurrentState = UnitState.Default;
                            });
                    });

                }).AddTo(this);
        }
        

        [PunRPC]
        private void PlayEffect(float x, float y, float z)
        {
            var slam = Instantiate(_slam);

            slam.transform.position = new Vector3(x, y, z);
            slam.Play();
            
            _audioSource.PlayOneShot(_clap);
            _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(slam.main.duration)).Subscribe(v =>
            {
                Destroy(slam.gameObject);
            });
        }
    }
}
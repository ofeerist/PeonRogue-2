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
        [SerializeField] private int _chargeRegenerateTime;
        
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
        private AxeAttack _axeAttack;

        private void Awake()
        {
            _serialDisposable.AddTo(this);
            _effectDisposable.AddTo(this);
        }
        
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _axeAttack = GetComponent<AxeAttack>();
            
            _animator = _unit.Animator;
            _photonView = _unit.PhotonView;

            _attackCooldownTimer = Time.time;

            if (!_photonView.IsMine) return;
            
            CurrentCharges = _maxCharges;
            Observable.Interval(TimeSpan.FromSeconds(_chargeRegenerateTime)).Subscribe(_ =>
            {
                if (CurrentCharges < _maxCharges) CurrentCharges++;
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
                        _axeAttack.LookPosition = hit.point;
                        _axeAttack.LookPosition.y = 0;
                    }

                    _attackCooldownTimer = _attackCooldown + Time.time;
                    CurrentCharges -= 1;

                    _animator.SetInteger(AttackNum, 2);
                    _animator.SetTrigger(Attack1);

                    var position = _attackPosition.position;
                    _unit.PhotonView.RPC(nameof(PlayEffect), RpcTarget.All, position.x,
                        position.y, position.z);

                    _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_slamDelay)).Subscribe(z =>
                    {
                        var size = Physics.OverlapSphereNonAlloc(_attackPosition.position, _attackRadius, _results, _layerMask);
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
            _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_slamDelay)).Subscribe(c =>
            {
                slam.transform.position = new Vector3(x, y, z);
                slam.Play();
                
                _audioSource.PlayOneShot(_clap);
                _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(slam.main.duration)).Subscribe(v =>
                {
                    Destroy(slam.gameObject);
                });
            });
        }
    }
}
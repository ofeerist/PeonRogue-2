using System;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    public class AxeThrow : MonoBehaviour
    {
        [SerializeField] private Axe _prefab;
        
        [Header("Regular")]
        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _speed;

        [Header("Roll")]
        [SerializeField] private float _damageRoll;
        [SerializeField] private float _knockbackRoll;
        [SerializeField] private float _speedRoll;

        [Space]

        [SerializeField] private int _maxThrowCharges;

        [Space]

        [SerializeField] private float _axeCreateOffset;
        [SerializeField] private float _stateEndOffset;
        
        private int _currentThrowCharges;
        public int CurrentThrowCharges
        {
            get
            {
                return _currentThrowCharges;
            }
            private set
            {
                _currentThrowCharges = value;
                ChargeChanged?.Invoke(value);
            }
        }

        [SerializeField] private float _chargeTimeToRegen;
        private float _refreshTime;
        public delegate void TimeChange(float time);
        public event TimeChange TimeChanged;

        public delegate void Tapping();
        public event Tapping Overtapping;

        [SerializeField] private float _maxFlightDistance;

        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;
        
        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;

        private Unit _unit;
        private Animator _animator;
        private PhotonView _photonView;
        private RollAttack _rollAttack;
        private Movement _movement;
        
        private static readonly int AttackNum = Animator.StringToHash("AttackNum");
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        private readonly SerialDisposable _attackDisposable = new SerialDisposable();

        private void Awake()
        {
            _attackDisposable.AddTo(this);
        }
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _animator = _unit.Animator;
            _photonView = _unit.PhotonView;
            _rollAttack = GetComponent<RollAttack>();
            _movement = GetComponent<Movement>();
            
            _attackCooldownTimer = 0;
            CurrentThrowCharges = _maxThrowCharges;
            
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (CurrentThrowCharges < _maxThrowCharges)
                {
                    _refreshTime += (1 / _chargeTimeToRegen) * Time.deltaTime;

                    var normalized = _refreshTime;
                    TimeChanged?.Invoke(normalized);
                    
                    if (normalized >= 1f)
                    {
                        _refreshTime = 0;

                        CurrentThrowCharges++;
                    }
                }
            }).AddTo(this);

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Mouse1))
                .Subscribe(x =>
                {
                    if (_unit.CurrentState == UnitState.Default &&
                        CurrentThrowCharges > 0 &&
                        _attackCooldownTimer < Time.time)
                    {
                        
                    }
                    else
                    {
                        Overtapping?.Invoke();
                        
                        return;
                    }
                    _unit.CurrentState = UnitState.Attack;
                    _attackCooldownTimer = _attackCooldown + Time.time;
                    CurrentThrowCharges -= 1;

                    _photonView.RPC(nameof(AnimatorTrigger), RpcTarget.AllViaServer);
                    
                    var ray = _unit.Camera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _movement.GroundLayer))
                    {
                        var position = transform.position;
                        var toPoint = (hit.point - position).normalized;
                        toPoint.y = 0;

                        _movement.LookPosition = hit.point;

                        var rotation = Quaternion.LookRotation(toPoint, Vector3.up);
                        
                        _attackDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_axeCreateOffset)).Subscribe(
                            z =>
                            {
                                _photonView.RPC(nameof(CreateAxe), RpcTarget.AllViaServer, position.x, position.y,
                                    position.z, rotation.x, rotation.y, rotation.z, rotation.w, _rollAttack.InRoll);
                            
                                _attackDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_stateEndOffset))
                                    .Subscribe(c =>
                                    {
                                        if (_unit.CurrentState == UnitState.Attack) _unit.CurrentState = UnitState.Default;
                                    });
                            });
                    }
                }).AddTo(this);
        }

        [PunRPC]
        private void AnimatorTrigger()
        {
            _animator.SetInteger(AttackNum, 2);
            _animator.SetTrigger(Attack1);
        }
        
        [PunRPC]
        private void CreateAxe(float x, float y, float z, float rx, float ry, float rz, float rw, bool roll)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            var position = new Vector3(x, y, z)  + new Vector3(0, 1, 0);
            var rotation = new Quaternion(rx, ry, rz, rw);
            if (!roll)
                Axe.Create(_prefab, _unit, position, rotation, _speed, _maxFlightDistance, _damage, _knockback);
            else
                Axe.Create(_prefab, _unit, position, rotation, _speedRoll, _maxFlightDistance, _damageRoll, _knockbackRoll, true);
        }
        
    }
}
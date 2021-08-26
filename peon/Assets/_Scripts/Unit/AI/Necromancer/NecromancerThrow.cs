using System;
using System.Collections;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI.Necromancer
{
    public class NecromancerThrow : MonoBehaviour
    {
        [SerializeField] private NecromancerProjectile _prefab;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private int _throws;
        [SerializeField] private float _angle;
        [SerializeField] private bool _clamped;

        [Space]

        [SerializeField] private float _minThrowDelay;
        [SerializeField] private float _maxThrowDelay;
        private float _currentThrowDelay;
        private float _throwDelayTimer;

        [SerializeField] private float _detectDistance;
        
        [Space]

        [SerializeField] private float _prjCreateOffset;
        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _speed;
        [SerializeField] private float _maxFlightDistance;
        [SerializeField] private float _damageRange;
        [SerializeField] private ParticleSystem _disposeEffect;

        private Unit _unit;
        private Animator _animator;
        private readonly Collider[] _results = new Collider[6];
        private static readonly int Attack = Animator.StringToHash("Attack");
        private MovementAI _movement;

        private readonly SerialDisposable _createDisposable = new SerialDisposable();
        private readonly SerialDisposable _stopDisposable = new SerialDisposable();
        
        [SerializeField] private AnimationClip _clip;
        private void Awake()
        {
            _createDisposable.AddTo(this);
            _stopDisposable.AddTo(this);
        }
        
        public void SetData(int throws, float angle, bool clamped, float detect, float minThrowDelay, float maxThrowDelay, float damage, float knockback, float speed, float maxFlightDistance)
        {
            _throws = throws;
            _angle = angle;
            _clamped = clamped;
            _detectDistance = detect;
            _minThrowDelay = minThrowDelay;
            _maxThrowDelay = maxThrowDelay;
            _damage = damage;
            _knockback = knockback;
            _speed = speed;
            _maxFlightDistance = maxFlightDistance;
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _animator = GetComponentInChildren<Animator>();
            _movement = GetComponent<MovementAI>();
            _throwDelayTimer = 0;

            if (!PhotonNetwork.IsMasterClient) return;
            
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                FindTarget();
            }).AddTo(this);
        }

        private void FindTarget()
        {
            if (_unit.CurrentState != UnitState.Default) return;

            if (_throwDelayTimer <= Time.time)
            {
                var _transform = transform;

                var size = Physics.OverlapSphereNonAlloc(_transform.position, _detectDistance, _results, _layerMask);

                Collider closest = null;
                var minDistance = Mathf.Infinity;
                for (int i = 0; i < size; i++)
                {
                    if (Vector3.Distance(_transform.position, _results[i].transform.position) < minDistance)
                    {
                        closest = _results[i];
                    }
                }

                if (closest != null) Throw(closest.GetComponent<Unit>(), _transform);
            }
        }

        private void Throw(Unit unit, Transform _transform)
        {
            _unit.CurrentState = UnitState.Attack;
            var position = unit.transform.position;
            _movement.ToTarget = position;
            
            _currentThrowDelay = Random.Range(_minThrowDelay, _maxThrowDelay);
            _throwDelayTimer = Time.time + _currentThrowDelay;
            _animator.SetTrigger(Attack);
            
            var toPoint = position - _transform.position;
            var rotation = Quaternion.LookRotation(toPoint, Vector3.up);

            if (_throws == 1)
            {
                PrjCreate(rotation);
            }
            else 
            {
                if (_clamped) SpawnClampFan(rotation, _angle, _throws);
                else SpawnFan(rotation, _angle, _throws);
            }

            _stopDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_clip.length)).Subscribe(x =>
            {
                if (_unit.CurrentState == UnitState.Attack) _unit.CurrentState = UnitState.Default;
            });

        }

        private void SpawnClampFan(Quaternion rotation, float angle, int throws)
        {
            var outsideRotation = rotation * Quaternion.Euler(0, -(angle / 2), 0);

            var offsetAngle = angle / (throws - 1);
            for (int i = 0; i < throws; i++)
            {
                var r = outsideRotation * Quaternion.Euler(0, offsetAngle * i, 0);
                PrjCreate(r);
            }
        }

        public void SpawnFan(Quaternion rotation, float angle, int throws)
        {
            if (throws % 2 == 1) PrjCreate(rotation);

            int throwPerSide;
            if (throws % 2 == 0) throwPerSide = _throws / 2;
            else throwPerSide = (throws - 1) / 2;

            var side = new [] { 1, -1 };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= throwPerSide; j++)
                {
                    var addAngle = angle * j * side[i] - (throws % 2 == 0 && j == 1 ? (angle / 2) * side[i] : 0);
                    PrjCreate(rotation * Quaternion.Euler(0, addAngle, 0));
                }
            }
        }

        private void PrjCreate(Quaternion rotation)
        {
            _createDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_prjCreateOffset)).Subscribe(x =>
            {
                NecromancerProjectile.Create(_prefab, transform.position + new Vector3(0, .4f, 0), rotation, _speed, _maxFlightDistance, _damage, _knockback);
            });
        }
    }
}

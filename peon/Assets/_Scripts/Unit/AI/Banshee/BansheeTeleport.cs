using System;
using System.Collections;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI.Banshee
{
    class BansheeTeleport : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        private readonly Collider[] _results = new Collider[6];
        
        [SerializeField] private float _distanceToRetreat;
        [SerializeField] private float _retreatRange;

        [SerializeField] private bool _chase;
        [SerializeField] private float _minDistanceToChase;
        [SerializeField] private float _maxDistanceToChase;
        [SerializeField] private float _chaseRange;

        [SerializeField] private float _teleportCooldown;
        private float _teleportCooldownTimer;

        [SerializeField] private ParticleSystem _teleportEffect;

        private Unit _target;
        
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _startTeleport;
        [SerializeField] private AudioClip _endTeleport;
        
        private TextTag.TextTag _textTag;

        private KinematicCharacterMotor _motor;
        private Unit _unit;

        private readonly SerialDisposable _effect1Disposable = new SerialDisposable();
        private readonly SerialDisposable _effect2Disposable = new SerialDisposable();
        
        private void Awake()
        {
            _effect1Disposable.AddTo(this);
            _effect2Disposable.AddTo(this);
        }
        
        public void SetData(float distanceToRetreat, float retreatRange, bool chase, float minDistanceToChase, float maxDistanceToChase, float chaseRange, float teleportCooldown)
        {
            _distanceToRetreat = distanceToRetreat;
            _retreatRange = retreatRange;
            _chase = chase;
            _minDistanceToChase = minDistanceToChase;
            _maxDistanceToChase = maxDistanceToChase;
            _chaseRange = chaseRange;
            _teleportCooldown = teleportCooldown;
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _motor = GetComponent<KinematicCharacterMotor>();

            _teleportCooldownTimer = 0;

            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                if (_unit.CurrentState != UnitState.Default) return;

                var _transform = transform;

                DetectEnemy(_transform);
            
                if (_target != null && _teleportCooldownTimer < Time.time)
                {
                    if (Vector3.Distance(_target.transform.position, _transform.position) <= _distanceToRetreat)
                    {
                        Teleport(_retreatRange, transform.position);
                    }
                    else if (_chase)
                    {
                        if (Vector3.Distance(_target.transform.position, _transform.position) >= _minDistanceToChase)
                        {
                            Teleport(_chaseRange, _target.transform.position);
                        }
                    }
                }
            }).AddTo(this);
        }

        private void DetectEnemy(Transform _transform)
        {
            if (_target == null || Vector3.Distance(_transform.position, _target.transform.position) > _distanceToRetreat || _target.CurrentState == UnitState.Dead)
            {
                _target = null;

                var size = Physics.OverlapSphereNonAlloc(_transform.position, _chase ? _maxDistanceToChase : _distanceToRetreat, _results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    var unit = _results[i].GetComponent<Unit>();
                    if (unit == null) continue;

                    if (unit.CurrentState != UnitState.Dead)
                    {
                        _target = unit;
                    }
                }
            }
        }

        private void Teleport(float range, Vector3 position)
        {
            _teleportCooldownTimer = Time.time + _teleportCooldown;

            _audioSource.PlayOneShot(_startTeleport);

            var eff1 = PhotonNetwork.Instantiate(_teleportEffect.name, transform.position,
                Quaternion.Euler(0, Random.Range(0, 360), 0));
            _effect1Disposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1f))
                .Subscribe(x => { Destroy(eff1); });

            _effect2Disposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.1f)).Subscribe(x =>
            {
                var end = RandomNavmeshLocation(range, position);
                _motor.SetPosition(end);

                var eff2 = PhotonNetwork.Instantiate(_teleportEffect.name, end,
                    Quaternion.Euler(0, Random.Range(0, 360), 0));

                _effect2Disposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.1f)).Subscribe(z =>
                {
                    Destroy(eff2);
                });
            });
        }

        private static Vector3 RandomNavmeshLocation(float radius, Vector3 offset)
        {
            var randomDirection = (Random.insideUnitSphere * radius) + offset;
            var finalPosition = Vector3.zero;

            if (NavMesh.SamplePosition(randomDirection, out var hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
    }
}

using System;
using _Scripts.Unit.Player;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.AI.Banshee
{
    public class BansheeShoutAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        private readonly Collider[] _results = new Collider[6];
        
        [SerializeField] private ParticleSystem _shoutEffect;
        [SerializeField] private ParticleSystem _prepareEffect;
        [SerializeField] private Transform _shoutPos;
        
        [Space]

        [SerializeField] private float _attackDistance;
        [SerializeField] private float _prepareTime;
        [SerializeField] private float _attackTime;
        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;
        
        [Space]

        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;

        [Space]
        
        private Unit _unit;

        private Unit _target;

        private ParticleSystem _shout;
        
        private AudioSource _audioSource;
        [SerializeField] private AudioClip _shoutClip;
        private static readonly int Shout1 = Animator.StringToHash("Shout");

        private readonly SerialDisposable _attackDisposable = new SerialDisposable();
        private readonly SerialDisposable _effectDisposable = new SerialDisposable();
        private MovementAI _movement;

        private void Awake()
        {
            _attackDisposable.AddTo(this);
            _effectDisposable.AddTo(this);
        }
        
        public void SetData(float attackDistance, float prepareTime, float attackTime, float attackCooldown, float damage, float knockback)
        {
            _attackDistance = attackDistance;
            _prepareTime = prepareTime;
            _attackTime = attackTime;
            _attackCooldown = attackCooldown;
            _damage = damage;
            _knockback = knockback;
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _movement = GetComponent<MovementAI>();
            
            _attackCooldownTimer = Time.time;

            _audioSource = GetComponent<AudioSource>();

            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                if (_unit.CurrentState != UnitState.Default) return;

                var _transform = transform;

                if (_attackCooldownTimer <= Time.time)
                {
                    var size = Physics.OverlapSphereNonAlloc(_transform.position, _attackDistance, _results, _layerMask);

                    Collider closest = null;
                    var minDistance = Mathf.Infinity;
                    for (int i = 0; i < size; i++)
                    {
                        if (Vector3.Distance(_transform.position, _results[i].transform.position) < minDistance)
                        {
                            closest = _results[i];
                        }
                    }

                    if (closest != null) Shout(closest.GetComponent<Unit>());
                }
            }).AddTo(this);
        }

        private void Shout(Unit closest)
        {
            _target = closest;
            _attackCooldownTimer = Time.time + _attackCooldown;

            _unit.CurrentState = UnitState.Attack;
            _movement.ToTarget = _target.transform.position;
            
            var _transform = transform;
            var position = _transform.position;
            var rotation = _transform.rotation;
            var eff = Instantiate(_prepareEffect, position, rotation);
            eff.Play();
            
            _attackDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_prepareTime)).Subscribe(z =>
            {
                eff.Stop();
                _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.3f)).Subscribe(v =>
                {
                    Destroy(eff.gameObject);
                });
                
                _unit.Animator.SetBool(Shout1, true);
                _audioSource.PlayOneShot(_shoutClip);

                //var shoutPos = position + new Vector3(0, .5f, 0) + _transform.forward;
                //var shoutRot = rotation * Quaternion.Euler(0, -90 - (_shoutEffect.shape.arc / 2), 0);
                _shout = Instantiate(_shoutEffect, _shoutPos.position, _shoutPos.rotation * Quaternion.Euler(0, -90, 0));
                _shout.Play();

                _attackDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
                {
                    var size = Physics.OverlapSphereNonAlloc(position, _attackDistance, _results, _layerMask);
                    for (int i = 0; i < size; i++)
                    {
                        var posTo = (_results[i].transform.position - transform.position).normalized;
                        var dot = Vector3.Dot(posTo, _transform.forward);
                        if (dot >= Mathf.Cos(_shoutEffect.shape.arc / 2 * Mathf.Deg2Rad))
                        {
                            var unit = _results[i].GetComponent<Unit>();
                            if (unit && unit.enabled)
                            {
                                unit.PhotonView.RPC(nameof(PlayerHealth.TakeDamage), RpcTarget.AllViaServer, _damage);

                                posTo *= _knockback;
                                unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                                    posTo.x, posTo.y, posTo.z);
                            }
                        }
                    }

                    _attackDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_attackTime)).Subscribe(c =>
                    {
                        _shout.Stop();
                        _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.3f)).Subscribe(v =>
                        {
                            Destroy(_shout.gameObject);
                            _shout = null;
                        });
                        
                        _unit.Animator.SetBool(Shout1, false);
                        if (_unit.CurrentState == UnitState.Attack) _unit.CurrentState = UnitState.Default;
                        
                        _target = null;
                    });
                });
            });
        }
    }
}

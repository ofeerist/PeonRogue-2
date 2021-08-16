using System;
using System.Collections;
using _Scripts.Unit.Player;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.AI.Skeleton
{
    public class DashSkeleton : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _dashMaxDetectRange;
        [SerializeField] private float _dashMinDetectRange;

        [SerializeField] private float _dashDamageRange;
        [SerializeField] private float _dashDamage;
        [SerializeField] private float _knockback;
        [SerializeField] private ParticleSystem _dashDamageEffect;

        [SerializeField] private float _dashSpeed;

        [SerializeField] private float _timeToPrepare;

        [SerializeField] private float _dashCooldown;
        private float _dashCooldownTimer;

        [SerializeField] private float _stopAnimationTime;

        [SerializeField] private ParticleSystem _dashDamageRangeEffect;

        private Unit _unit;

        private Vector3 _aimPos;
        private ParticleSystem _aimEffect;
        private ParticleSystem _damageEffect;
        private MovementAI _movement;
        private PhotonView _photonView;
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private readonly SerialDisposable _effectDisposable = new SerialDisposable();
        
        private readonly Collider[] _results = new Collider[6];
        
        public void SetData(float maxDetect, float minDetect, float range, float damage, float speed, float prepareTime, float cooldown)
        {
            _dashMaxDetectRange = maxDetect;
            _dashMinDetectRange = minDetect;
            _dashDamageRange = range;
            _dashDamage = damage;
            _dashSpeed = speed;
            _timeToPrepare = prepareTime;
            _dashCooldown = cooldown;
        }

        private void Awake()
        {
            _serialDisposable.AddTo(this);
            _effectDisposable.AddTo(this);
        }

        private void Start()
        {
            _dashCooldownTimer = 0;

            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();

            if (!PhotonNetwork.IsMasterClient) return;
            
            _movement = _unit.GetComponent<MovementAI>();
            _movement.ByMovementHit += (hitCollider, normal, point) =>
            {
                var layer = hitCollider.gameObject.layer;
                if (layer != 8 && layer != 9 && layer != 11) return;
                if (_unit.CurrentState == UnitState.Dash) _unit.CurrentState = UnitState.Default;
            };
            
            _aimPos = Vector3.zero;

            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                if (_unit.CurrentState != UnitState.Default || _dashCooldownTimer > Time.time) return;
                
                var size = Physics.OverlapSphereNonAlloc(transform.position, _dashMaxDetectRange, _results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    var targetPosition = _results[i].transform.position;
                    
                    if (Vector3.Distance(targetPosition, transform.position) < _dashMinDetectRange) continue;

                    if (_results[i].GetComponent<Unit>().enabled) 
                    {
                        Dash(targetPosition);

                        break;
                    }
                }
            }).AddTo(this);
        }

        [PunRPC]
        private void StartEffect(float x, float y, float z) =>
            _aimEffect = Instantiate(_dashDamageRangeEffect, new Vector3(x, y, z), Quaternion.identity);

        [PunRPC]
        private void DestroyEffect()
        {
            _aimEffect.Stop();
            var eff = _aimEffect;
            _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(.4f)).Subscribe(z => { Destroy(eff.gameObject); });
        }

        [PunRPC]
        private void StartDamageEffect(float x, float y, float z)
        {
            _damageEffect = Instantiate(_dashDamageEffect);
            _damageEffect.transform.position = new Vector3(x, y, z);
            _damageEffect.Play();
        }

        [PunRPC]
        private void DestroyDamageEffect()
        {
            var eff = _damageEffect;
            _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(z =>
            {
                eff.Stop();
                Destroy(eff.gameObject);
            });
        }
        
        private void Dash(Vector3 targetPosition)
        {
            _unit.CurrentState = UnitState.Dash;
            
            _aimPos = targetPosition;
            _dashCooldownTimer = Time.time + _dashCooldown;

            _photonView.RPC(nameof(StartEffect), RpcTarget.AllViaServer, _aimPos.x, _aimPos.y, _aimPos.z);

            _unit.Animator.SetTrigger(Attack1);

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_stopAnimationTime)).Subscribe(c =>
            {
                _unit.Animator.speed = 0;

                _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_timeToPrepare)).Subscribe(x =>
                {
                    _photonView.RPC(nameof(DestroyEffect), RpcTarget.AllViaServer);

                    _serialDisposable.Disposable = Observable.FromMicroCoroutine(ProccessMove).Subscribe(z =>
                    {
                        _unit.Animator.speed = 1;
                    });
                });
            });
        }
        
        private IEnumerator ProccessMove()
        {
            var _transform = transform;
            var position = _transform.position;
            var dir = (_aimPos - position).normalized;
            
            var lastDistance = Mathf.Infinity;
            var distance = Vector3.Distance(position, _aimPos);
            
            while (distance <= lastDistance)
            {
                if (_unit.CurrentState != UnitState.Dash) yield break;
                
                var velocity = dir * _dashSpeed;
                velocity.y = 0;
                
                _movement.AddVelocity(velocity);
                
                lastDistance = distance;
                
                yield return null;
                
                position = _transform.position;
                distance = Vector3.Distance(position, _aimPos);
            }

            DoDamage();
            _unit.CurrentState = UnitState.Default;
        }

        private void DoDamage()
        {
            var position = transform.position;
            _photonView.RPC(nameof(StartDamageEffect), RpcTarget.AllViaServer, position.x, position.y, position.z);
            _photonView.RPC(nameof(DestroyDamageEffect), RpcTarget.AllViaServer);
            
            var size = Physics.OverlapSphereNonAlloc(position, _dashDamageRange, _results, _layerMask);
            for (int i = 0; i < size; i++)
            {
                var unit = _results[i].GetComponent<Unit>();
                var posTo = (unit.transform.position - transform.position).normalized;
                
                if (unit.enabled)
                {
                    unit.PhotonView.RPC(nameof(PlayerHealth.TakeDamage), RpcTarget.AllViaServer, _dashDamage);

                    posTo.y = 0;
                    posTo *= _knockback;
                    unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                        posTo.x, posTo.y, posTo.z);
                }
            }
        }
    }
}
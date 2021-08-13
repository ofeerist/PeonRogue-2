using System;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = System.Random;

namespace _Scripts.Unit.Player
{
    public class DashAttack : MonoBehaviour
    {
        private Unit _unit;

        private bool _attack;
        
        [SerializeField] private float _damageInterval;
        [SerializeField] private float _dashAttackRange;
        [SerializeField] private float _dashAttackKnockback;
        [SerializeField] private float _dashAttackDamage;
        [SerializeField] private float _dashAttackAngle;
        
        [Space]
        
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _dashHit;
        [SerializeField] private ParticleSystem _dashAttackEffect;
        
        private static readonly int Attack = Animator.StringToHash("DashAttack");
        [SerializeField] private LayerMask _layerMask;
        private PhotonView _photonView;

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();

            if (!_photonView.IsMine) return;
            
            Observable.EveryUpdate() 
                .Where(_ => Input.GetKeyDown(KeyCode.Mouse0))
                .Subscribe (x =>
                {
                    if (_unit.CurrentState != PlayerState.Dash) return;
                    _photonView.RPC(nameof(Proccess), RpcTarget.AllViaServer);
                }).AddTo (this); 
            
            Observable.Interval(TimeSpan.FromSeconds(_damageInterval)).Subscribe(_ =>
            {
                if (_attack && _unit.CurrentState == PlayerState.Dash)
                {
                    AttackDamage();
                }
                else
                {
                    _unit.Animator.SetBool(Attack, false);
                }
            }).AddTo(this);
        }
        
        [PunRPC]
        private void Proccess()
        {
            _dashAttackEffect.Play();
            _attack = true;
            _unit.Animator.SetBool(Attack, _attack);
        }
        
        private void AttackDamage()
        {
            var transform1 = transform;
            var position = transform1.position;
            var forward = transform1.forward;
            
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(position, _dashAttackRange, results, _layerMask);
            for (int i = 0; i < size; i++)
            {
                var unit = results[i].GetComponent<Unit>();
                if (unit != null && unit.UnitHealth != null && unit.enabled)
                {
                    var posTo = (unit.transform.position - position).normalized;
                    var dot = Vector3.Dot(posTo, forward);
                    if (dot >= Mathf.Cos(_dashAttackAngle))
                    {
                        unit.PhotonView.RPC(nameof(unit.UnitHealth.TakeDamage), RpcTarget.AllViaServer,
                            _dashAttackDamage);
                        _photonView.RPC(nameof(PlayHit), RpcTarget.AllViaServer, new Random().Next(0));

                        posTo.y = 0;
                        unit.UnitMovement.AddImpulse((posTo) * _dashAttackKnockback);
                    }
                }
            }
        }

        [PunRPC]
        private void PlayHit(int seed)
        {
            _audioSource.PlayOneShot(_dashHit[new Random(seed).Next(0, _dashHit.Length)]);
        }
    }
}
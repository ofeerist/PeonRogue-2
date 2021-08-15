using System;
using _Scripts.Unit.Player;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI
{
    public class MeleeAIAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _speed;
        [SerializeField] private float _range;
        [SerializeField] private float _angle;
        [SerializeField] private float _damage;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _preAttack;
        [SerializeField] private AudioClip[] _hit;

        private float _attackCooldown;

        private TextTag.TextTag _textTag;
        
        private static readonly int Attack1 = Animator.StringToHash("Attack");
        
        private PhotonView _photonView;
        private Unit _unit;

        [HideInInspector] public Vector3 ToTarget;
        
        public void SetData(float attackSpeed, float range, float angle, float damage)
        {
            _speed = attackSpeed;
            _range = range;
            _angle = angle;
            _damage = damage;
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();

            if (!PhotonNetwork.IsMasterClient) return;
            
            Observable.Interval(TimeSpan.FromSeconds(.1f)).Subscribe(x =>
            {
                FindTarget();
            }).AddTo(this);
        }

        private void FindTarget()
        {
            if (!_unit.enabled || _unit.CurrentState != UnitState.Default) return;

            if (_attackCooldown + _speed <= Time.time)
            {
                var results = new Collider[6];
                var size = Physics.OverlapSphereNonAlloc(transform.position, _range, results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    if (results[i].GetComponent<Unit>().enabled)
                    {
                        ToTarget = (results[i].transform.position - transform.position).normalized;
                        Attack();
                        
                        return;
                    }
                }
            }
        }
        
        private void Attack()
        {
            _unit.CurrentState = UnitState.Attack;
            _unit.Animator.SetTrigger(Attack1);
            
            var state = _unit.Animator.GetCurrentAnimatorStateInfo(0);

            var first = state.length * .3f;
            var second = state.length * .7f;
            
            Observable.Timer(TimeSpan.FromSeconds(first)).Subscribe(x =>
            {
                if(_unit.CurrentState == UnitState.Attack) DoDamage(_damage);
                
                Observable.Timer(TimeSpan.FromSeconds(second)).Subscribe(z =>
                    {
                        _attackCooldown = Time.time; 
                        if(_unit.CurrentState == UnitState.Attack)
                            _unit.CurrentState = UnitState.Default; 
                    }).AddTo(this);
            }).AddTo(this);

            _photonView.RPC(nameof(PlayPreAttack), RpcTarget.AllViaServer, Random.Range(0, 100));
        }

        [PunRPC]
        private void PlayPreAttack(int seed) =>
            _audioSource.PlayOneShot(_preAttack[new System.Random(seed).Next(0, _preAttack.Length)]);

        [PunRPC]
        private void PlayHit(int seed) =>
            _audioSource.PlayOneShot(_hit[new System.Random(seed).Next(0, _hit.Length)]);

        [PunRPC]
        private void SetTexttag(float x, float y, float z)
        {
            var position = new Vector3(x, y, z);
            
            if (_textTag == null)
                _textTag = TextTag.TextTag.Create(position, "Промах!", UnityEngine.Color.red, 1, new Vector3(0, .005f), false, 0.3f);
            else
            {
                _textTag.transform.position = position;
                _textTag.Color = UnityEngine.Color.red;
            }
        }

        private void DoDamage(float damage)
        {
            var _transform = transform;
            
            var results = new Collider[6];
            var size = Physics.OverlapSphereNonAlloc(_transform.position, _range, results, _layerMask);
            int damaged = 0;
            
            for (int i = 0; i < size; i++)
            {
                var unit = results[i].GetComponent<Unit>();
                
                var posTo = (unit.transform.position - _transform.position).normalized;
                var dot = Vector3.Dot(posTo, _transform.forward);
                if (dot >= Mathf.Cos(_angle))
                {
                    unit.PhotonView.RPC(nameof(PlayerHealth.TakeDamage), RpcTarget.AllViaServer, damage);
                    _photonView.RPC(nameof(PlayHit), RpcTarget.AllViaServer, Random.Range(0, 100));
                    
                    damaged++;
                } 
            }

            if(damaged == 0)
            {
                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
                var p = _transform.position + randomOffset;
                _photonView.RPC(nameof(SetTexttag), RpcTarget.AllViaServer, p.x, p.y, p.z);
            }
        }
    }
}
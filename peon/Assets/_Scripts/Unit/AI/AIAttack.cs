using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI
{
    class AIAttack : UnitAttack
    {
        [SerializeField] private LayerMask _layerMask;

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

        private Coroutine _attack;
        
        public void SetData(float attackSpeed, float range, float angle, float damage)
        {
            _Speed = attackSpeed;
            _range = range;
            _angle = angle;
            _damage = damage;
        }

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();

            InvokeRepeating(nameof(FindTarget), 1, .1f);
        }

        private void FindTarget()
        {
            if (!Unit.enabled) return;

            if (!PhotonNetwork.IsMasterClient) return;

            if (_attack != null) return;
            
            if (_attackCooldown + _Speed <= Time.time)
            {
                print('d');
                var results = new Collider[10];
                var size = Physics.OverlapSphereNonAlloc(transform.position, _range, results, _layerMask);
                for (int i = 0; i < size; i++)
                {
                    if (results[i].GetComponent<Unit>().enabled)
                    {
                        Unit.UnitMovement.BlockMovement = true;
                        
                        _photonView.RPC(nameof(Attack), RpcTarget.All);
                        return;
                    }
                }
            }
        }

        [PunRPC]
        private void Attack()
        {
            Unit.Animator.SetTrigger(Attack1);
            _attack = StartCoroutine(DoAttack());
            _audioSource.PlayOneShot(_preAttack[Random.Range(0, _preAttack.Length)]);
        }

        private IEnumerator DoAttack()
        {
            var state = Unit.Animator.GetCurrentAnimatorStateInfo(0);

            var first = state.length * .3f;
            var second = state.length * .7f;
            
            yield return new WaitForSeconds(first);
            
            DoDamage(_damage);

            yield return new WaitForSeconds(second);

            _attack = null;
            _attackCooldown = Time.time;
        }
        
        private void DoDamage(float damage)
        {
            var _transform = transform;
            
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(_transform.position, _range, results, _layerMask);
            int damaged = 0;
            
            for (int i = 0; i < size; i++)
            {
                var unit = results[i].GetComponent<Unit>();
                
                var posTo = (unit.transform.position - _transform.position).normalized;
                var dot = Vector3.Dot(posTo, _transform.forward);
                if (dot >= Mathf.Cos(_angle))
                {
                    unit.UnitHealth.TakeDamage(damage);
                    damaged++;
                    _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);
                } 
            }

            if(damaged == 0)
            {
                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
                if (_textTag == null)
                    _textTag = TextTag.TextTag.Create(_transform.position + randomOffset, "Промах!", UnityEngine.Color.red, 1, new Vector3(0, .005f), false, 0.3f);
                else
                {
                    _textTag.transform.position = transform.position + randomOffset;
                    _textTag.Color = UnityEngine.Color.red;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var _transform = transform;

            Gizmos.color = UnityEngine.Color.red;

            var position = _transform.position;
            var eulerAngles = _transform.eulerAngles;
            var vectorTo1 = new Vector3(position.x + _range * Mathf.Sin((_angle + eulerAngles.y) * Mathf.Deg2Rad), position.y, position.z + _range * Mathf.Cos((_angle + eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(position, vectorTo1);

            var vectorTo2 = new Vector3(position.x + _range * Mathf.Sin((-_angle + eulerAngles.y) * Mathf.Deg2Rad), position.y, position.z + _range * Mathf.Cos((-_angle + eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(position, vectorTo2);

            for (float j = 1; j <= 10; j += .2f)
            {
                var angles = _transform.eulerAngles;
                var position1 = _transform.position;
                vectorTo1 = new Vector3(position1.x + _range * Mathf.Sin((_angle / j + angles.y) * Mathf.Deg2Rad), position1.y, position1.z + _range * Mathf.Cos((_angle / j + angles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(position1.x + _range * Mathf.Sin((_angle / (j + .2f) + angles.y) * Mathf.Deg2Rad), position1.y, position1.z + _range * Mathf.Cos((_angle / (j + .2f) + angles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                vectorTo1 = new Vector3(position1.x + _range * Mathf.Sin((-_angle / j + angles.y) * Mathf.Deg2Rad), position1.y, position1.z + _range * Mathf.Cos((-_angle / j + angles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(position1.x + _range * Mathf.Sin((-_angle / (j + .2f) + angles.y) * Mathf.Deg2Rad), position1.y, position1.z + _range * Mathf.Cos((-_angle / (j + .2f) + angles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                if (j + .2f > 10)
                {
                    vectorTo1 = new Vector3(_transform.position.x + _range * Mathf.Sin((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    vectorTo2 = new Vector3(_transform.position.x + _range * Mathf.Sin((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    Gizmos.DrawLine(vectorTo1, vectorTo2);
                }
            }
        }
    }
}

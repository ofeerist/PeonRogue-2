using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Game.Unit
{
    class AIMovement : UnitMovement
    {
        [SerializeField] private float _detectionRange;

        private Unit _target;
        private NavMeshAgent _navMeshAgent;

        public float BounceDamage;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;
        }

        private void Update()
        {
            if (Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || !Unit.enabled || Unit.UnitHealth.InStan) BlockMovement = true;
            else BlockMovement = false;

            if (!Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                Unit.Animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);      
            else        
                Unit.Animator.SetFloat("Speed", 0);           

            var _transform = transform;
            DetectEnemy(_transform);

            if(Unit.enabled && _navMeshAgent.enabled)
                if (!BlockMovement)
                {
                    if(_target != null) _navMeshAgent.SetDestination(_target.transform.position);
                }
                else
                {
                    _navMeshAgent.SetDestination(transform.position);
                    Unit.Animator.SetFloat("Speed", 0);
                }


            if (Unit.Rigidbody.velocity.magnitude <= 0.1f)
            {
                _navMeshAgent.enabled = true;
                Unit.Rigidbody.isKinematic = true;
            }
            else
            {
                _navMeshAgent.enabled = false;
            }
        }

        public override void AddImpulse(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            _navMeshAgent.enabled = false;
            Unit.Rigidbody.isKinematic = false;
            base.AddImpulse(direction, stan, stanTime);
        }

        private void DetectEnemy(Transform _transform)
        {
            if (_target == null || Vector3.Distance(_transform.position, _target.transform.position) > _detectionRange || !_target.enabled)
            {
                _target = null;

                var objects = Physics.OverlapSphere(_transform.position, _detectionRange);
                var enemys = new List<Unit>();
                foreach (var obj in objects)
                {
                    if (obj.CompareTag("Player")) enemys.Add(obj.GetComponent<Unit>());
                }

                Unit closest = null;
                var minDistance = Mathf.Infinity;
                foreach (var enemy in enemys)
                {
                    if (Vector3.Distance(_transform.position, enemy.transform.position) < minDistance)
                    {
                        closest = enemy;
                    }
                }

                if(closest != null) _target = closest;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 9 && !_navMeshAgent.enabled && enabled)
            {
                Unit.UnitHealth.TakeDamage(BounceDamage);

                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
                TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", Color.gray, .5f, new Vector3(0, .005f), 0.25f);
            }
        }
    }
}

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

                    if (Physics.SphereCast(_transform.position, _navMeshAgent.radius, _transform.forward, out var hit, Mathf.Infinity))
                    {
                        if(hit.collider.TryGetComponent<NavMeshAgent>(out var target))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                var next = _navMeshAgent.nextPosition;
                                var targetNext = target.nextPosition;

                                if (Vector3.Distance(next, targetNext) < _navMeshAgent.radius)
                                {
                                    _navMeshAgent.velocity = MoveVelocity(_navMeshAgent.velocity, target.velocity);
                                    _navMeshAgent.velocity = Vector3.ClampMagnitude(_navMeshAgent.velocity, _navMeshAgent.speed);
                                }
                            }
                        }
                    }
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

        private Vector3 MoveVelocity(Vector3 v1, Vector3 v2)
        {
            var finV1 = v1;
            var v1Dot = Vector3.Dot(v1.normalized, v2.normalized);
            var dot = v1Dot / (v1.magnitude / v2.magnitude);

            finV1 -= v2.normalized * v1Dot;
            return finV1;
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
                TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", Color.gray, 5, new Vector3(0, .005f), 0.3f);
            }
        }
    }
}

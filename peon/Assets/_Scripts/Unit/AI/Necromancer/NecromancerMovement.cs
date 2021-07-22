using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Unit
{
    class NecromancerMovement : UnitMovement
    {
        [SerializeField] private float _retreatDistance;
        [SerializeField] private bool _attackOnClamp;

        [Space]

        [SerializeField] private bool _chase;
        [SerializeField] private float _minDistanceToChase;
        [SerializeField] private float _maxDistanceToChase;

        private Transform _target;
        private NavMeshAgent _navMeshAgent;

        public float BounceDamage;

        private bool _clamped;
        [SerializeField] private float _clampResetTime;

        private NecromancerThrow _throw;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _throw = GetComponent<NecromancerThrow>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;
        }

        protected override void OnTick()
        {
            Unit.Animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);

            var _transform = transform;

            DetectEnemy(_transform);

            if (Unit.enabled && _navMeshAgent.enabled)
                if (!BlockMovement)
                {
                    if (_target != null)
                    {
                        if (Vector3.Distance(_target.transform.position, _transform.position) <= _retreatDistance)
                        {
                            var v = transform.position + (_transform.position - _target.position).normalized * _retreatDistance;
                            if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                                _navMeshAgent.SetDestination(v);


                            if (!_clamped && !_navMeshAgent.CalculatePath(v, _navMeshAgent.path) && _navMeshAgent.velocity.magnitude == 0f)
                            {
                                _clamped = true;
                                StartCoroutine(ResetClamped(_clampResetTime));
                                _navMeshAgent.SetDestination(RandomNavmeshLocation(_retreatDistance));

                                if (_attackOnClamp)
                                {
                                    _throw.SpawnFan(transform.rotation, 30, 11);
                                }
                            }
                        }
                        else if(_chase)
                        {
                            if(Vector3.Distance(_target.transform.position, _transform.position) > _minDistanceToChase)
                                _navMeshAgent.SetDestination(_target.transform.position);
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

            if (Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || !Unit.enabled || Unit.UnitHealth.InStan) BlockMovement = true;
            else BlockMovement = false;
        }

        private IEnumerator ResetClamped(float time)
        {
            yield return new WaitForSeconds(time);
            _clamped = false;
        }

        private Vector3 RandomNavmeshLocation(float radius)
        {
            var randomDirection = (Random.insideUnitSphere * radius) + transform.position;
            Vector3 finalPosition = Vector3.zero;

            if (NavMesh.SamplePosition(randomDirection, out var hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        public override void AddImpulse(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            _navMeshAgent.enabled = false;
            Unit.Rigidbody.isKinematic = false;
            base.AddImpulse(direction, stan, stanTime);
        }

        private void DetectEnemy(Transform _transform)
        {
            if (_target == null || Vector3.Distance(_transform.position, _target.position) > _retreatDistance || !_target.GetComponent<Unit>().isActiveAndEnabled)
            {
                _target = null;

                var objects = Physics.OverlapSphere(_transform.position, _chase ? _maxDistanceToChase : _retreatDistance);
                foreach (var item in objects)
                {
                    var unit = item.GetComponent<Unit>();
                    if (unit == null) continue;

                    if (unit.CompareTag("Player") && unit.enabled)
                    {
                        _target = unit.transform;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, _retreatDistance);

            if (_target != null)
            {
                var v2 = transform.position + (transform.position - _target.position).normalized;
                Gizmos.DrawLine(transform.position, v2);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 9 && !_navMeshAgent.enabled)
            {
                Unit.UnitHealth.TakeDamage(BounceDamage);

                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
                TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", Color.gray, 1, new Vector3(0, .005f), 0.2f);
            }
        }
    }
}

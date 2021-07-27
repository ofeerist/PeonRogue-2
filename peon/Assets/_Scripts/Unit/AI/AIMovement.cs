using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Game.Unit
{
    class AIMovement : UnitMovement
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _detectionRange;

        private Unit _target;
        private NavMeshAgent _navMeshAgent;

        public float BounceDamage;
        private TextTag.TextTag _textTag;

        public void SetData(float detectionRange, float rotateSpeed, float speed)
        {
            _detectionRange = detectionRange;
            _rotateSpeed = rotateSpeed;
            Speed = speed;
        }

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;

            InvokeRepeating(nameof(FindTarget), 1, .5f);

            InvokeRepeating(nameof(UpdateAnim), 1, .1f);
        }

        private void UpdateAnim()
        {
            if (!Unit.enabled) return;

            if (!Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                Unit.Animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);
            else
                Unit.Animator.SetFloat("Speed", 0);
        }

        private void FindTarget()
        {
            if (!Unit.enabled) return;

            var _transform = transform;
            DetectEnemy(_transform);

            if (Unit.enabled && _navMeshAgent.enabled)
                if (!BlockMovement)
                {
                    if (_target != null) _navMeshAgent.SetDestination(_target.transform.position);
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

        protected override void OnTick()
        {
            if (Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || !Unit.enabled || Unit.UnitHealth.InStan) BlockMovement = true;
            else BlockMovement = false;      
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

                var objects = Physics.OverlapSphere(_transform.position, _detectionRange, _layerMask);

                Collider closest = null;
                var minDistance = Mathf.Infinity;
                for (int i = 0; i < objects.Length; i++)
                {
                    if (Vector3.Distance(_transform.position, objects[i].transform.position) < minDistance)
                    {
                        closest = objects[i];
                    }
                }

                if(closest != null) _target = closest.GetComponent<Unit>();
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

                if (_textTag == null)
                    _textTag = TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", Color.gray, 1, new Vector3(0, .005f), false, 0.2f);
                else
                {
                    _textTag.transform.position = transform.position + randomOffset;
                    _textTag.Color = Color.gray;
                }
            }
        }
    }
}

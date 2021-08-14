﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.Unit.AI.Necromancer
{
    class NecromancerMovement : UnitMovement
    {
        [SerializeField] private LayerMask _layerMask;

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
        private TextTag.TextTag _textTag;

        public void SetData(float retreatDisance, bool chase, float minDistanceToChase, float maxDistanceToChase, bool attackOnClamp, float rotateSpeed, float speed)
        {
            _retreatDistance = retreatDisance;
            _chase = chase;
            _minDistanceToChase = minDistanceToChase;
            _maxDistanceToChase = maxDistanceToChase;
            _attackOnClamp = attackOnClamp;
            _rotateSpeed = rotateSpeed;
            Speed = speed;
        }

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _throw = GetComponent<NecromancerThrow>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;

            InvokeRepeating(nameof(UpdateDestination), 1f, .5f);

            InvokeRepeating(nameof(UpdateAnimation), 1f, .5f);
        }

        private void UpdateAnimation()
        {
            if (!Unit.enabled) return;

            Unit.Animator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);

            if (Unit.Rigidbody.velocity.magnitude <= 0.1f)
            {
                _navMeshAgent.enabled = true;
                Unit.Rigidbody.isKinematic = true;
            }
        }

        private void UpdateDestination()
        {
            if (!Unit.enabled) return;

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
                        else if (_chase)
                        {
                            if (Vector3.Distance(_target.transform.position, _transform.position) > _minDistanceToChase)
                                _navMeshAgent.SetDestination(_target.transform.position);
                        }
                    }
                }
                else
                {
                    _navMeshAgent.SetDestination(transform.position);
                    Unit.Animator.SetFloat("Speed", 0);
                }
        }

        protected override void OnTick()
        {
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
            if (_target == null || Vector3.Distance(_transform.position, _target.position) > _retreatDistance)
            {
                _target = null;
                
                var results = new Collider[10];
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _chase ? _maxDistanceToChase : _retreatDistance, results, _layerMask);
                for(int i = 0; i < size;)
                {           
                    _target = results[i].transform;
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.red;

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

                if (_textTag == null)
                    _textTag = TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", UnityEngine.Color.gray, 1, new Vector3(0, .005f), false, 0.2f);
                else
                {
                    _textTag.transform.position = transform.position + randomOffset;
                    _textTag.Color = UnityEngine.Color.gray;
                }
            }
        }
    }
}
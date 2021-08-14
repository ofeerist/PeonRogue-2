﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.Unit.AI.Banshee
{
    class BansheeMovement : UnitMovement
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _distanceToRetreat;
        [SerializeField] private float _retreatRange;

        [SerializeField] private bool _chase;
        [SerializeField] private float _minDistanceToChase;
        [SerializeField] private float _maxDistanceToChase;
        [SerializeField] private float _chaseRange;

        [SerializeField] private float _teleportCooldown;
        private float _teleportCooldownTimer;

        [SerializeField] private ParticleSystem _teleportEffect;

        private Transform _target;
        private NavMeshAgent _navMeshAgent;

        public float BounceDamage;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _startTeleport;
        [SerializeField] private AudioClip _endTeleport;
        private TextTag.TextTag _textTag;

        public void SetData(float distanceToRetreat, float retreatRange, bool chase, float minDistanceToChase, float maxDistanceToChase, float chaseRange, float teleportCooldown)
        {
            _distanceToRetreat = distanceToRetreat;
            _retreatRange = retreatRange;
            _chase = chase;
            _minDistanceToChase = minDistanceToChase;
            _maxDistanceToChase = maxDistanceToChase;
            _chaseRange = chaseRange;
            _teleportCooldown = teleportCooldown;
        }

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;

            _teleportCooldownTimer = 0;

            InvokeRepeating(nameof(FindTarget), 1f, .5f);

            InvokeRepeating(nameof(UpdateAnimation), 1f, .2f);
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
            else
            {
                _navMeshAgent.enabled = false;
            }
        }

        private void FindTarget()
        {
            if (!Unit.enabled) return;

            var _transform = transform;

            DetectEnemy(_transform);

            if (Unit.enabled && _navMeshAgent.enabled)
                if (!BlockMovement)
                {
                    if (_target != null && _teleportCooldownTimer < Time.time)
                    {
                        if (Vector3.Distance(_target.transform.position, _transform.position) <= _distanceToRetreat)
                        {
                            StartCoroutine(Teleport(_retreatRange, transform.position));
                        }
                        else if (_chase)
                        {
                            if (Vector3.Distance(_target.transform.position, _transform.position) >= _minDistanceToChase)
                            {
                                StartCoroutine(Teleport(_chaseRange, _target.transform.position));
                            }
                        }
                    }
                }
                else
                {
                    Unit.Animator.SetFloat("Speed", 0);
                }
        }

        protected override void OnTick()
        {
            if (!Unit.enabled) return;

            if (Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || Unit.UnitHealth.InStan || Unit.UnitAttack.InAttack) BlockMovement = true;
            else BlockMovement = false;
        }

        private IEnumerator Teleport(float range, Vector3 position)
        {
            _teleportCooldownTimer = Time.time + _teleportCooldown;

            _audioSource.PlayOneShot(_startTeleport);

            var eff = Instantiate(_teleportEffect, transform.position, Quaternion.identity);
            eff.Play();
            StartCoroutine(DestroyTimed(eff.gameObject, 1f));

            yield return new WaitForSeconds(.1f);

            _navMeshAgent.Warp(RandomNavmeshLocation(range, position));

            StartCoroutine(DelayedTeleportEffect());
        }

        private IEnumerator DelayedTeleportEffect()
        {
            yield return null;
            var eff = Instantiate(_teleportEffect, transform.position, Quaternion.identity);
            eff.Play();
            StartCoroutine(DestroyTimed(eff.gameObject, 1f));

            _audioSource.PlayOneShot(_endTeleport);
        }

        private Vector3 RandomNavmeshLocation(float radius, Vector3 offset)
        {
            var randomDirection = (Random.insideUnitSphere * radius) + offset;
            Vector3 finalPosition = Vector3.zero;

            if (NavMesh.SamplePosition(randomDirection, out var hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        private IEnumerator DestroyTimed(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(obj);
        }

        public override void AddImpulse(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            _navMeshAgent.enabled = false;
            Unit.Rigidbody.isKinematic = false;
            base.AddImpulse(direction, stan, stanTime);
        }

        private void DetectEnemy(Transform _transform)
        {
            if (_target == null || Vector3.Distance(_transform.position, _target.position) > _distanceToRetreat || !_target.GetComponent<Unit>().isActiveAndEnabled)
            {
                _target = null;

                var objects = Physics.OverlapSphere(_transform.position, _chase ? _maxDistanceToChase : _distanceToRetreat, _layerMask);
                foreach (var item in objects)
                {
                    var unit = item.GetComponent<Unit>();
                    if (unit == null) continue;

                    if (unit.enabled)
                    {
                        _target = unit.transform;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireSphere(transform.position, _distanceToRetreat);

            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _maxDistanceToChase);
            Gizmos.color = UnityEngine.Color.black;
            Gizmos.DrawWireSphere(transform.position, _minDistanceToChase);
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
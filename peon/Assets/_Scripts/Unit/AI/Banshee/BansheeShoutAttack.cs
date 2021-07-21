using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Unit
{
    class BansheeShoutAttack : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _shoutEffect;
        [SerializeField] private ParticleSystem _prepareEffect;

        [Space]

        [SerializeField] private float _attackDistance;
        [SerializeField] private float _prepareTime;
        [SerializeField] private float _attackTime;
        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;
        [SerializeField] private float _damageDistance;

        [Space]

        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;

        [Space]

        [SerializeField] private float _rotateSpeed;

        private Unit _unit;

        private Unit _target;

        private ParticleSystem _shout;
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _attackCooldownTimer = Time.time;
        }

        private void Update()
        {
            if (_unit.UnitAttack.InAttack) return;

            var _transform = transform;

            if (_attackCooldownTimer <= Time.time)
            {
                var objects = Physics.OverlapSphere(_transform.position, _attackDistance);
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

                if (closest != null) StartCoroutine(Shout(closest));
            }
        }

        private void FixedUpdate()
        {
            if (_shout != null) return;

            var _transform = transform;
            if (_target != null)
            {
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_target.transform.position - _transform.position), _rotateSpeed * Time.fixedDeltaTime);
            }
        }

        private IEnumerator Shout(Unit closest)
        {
            _target = closest;
            _attackCooldownTimer = Time.time + _attackCooldown;

            _unit.UnitAttack.InAttack = true;

            var eff = Instantiate(_prepareEffect, transform.position, transform.rotation);
            eff.Play();

            yield return new WaitForSeconds(_prepareTime);

            eff.Stop();
            StartCoroutine(DestoyTimed(.5f, eff.gameObject));

            _unit.Animator.SetBool("Shout", true);

            var shoutPos = transform.position + new Vector3(0, .5f, 0) + transform.forward;
            var shoutRot = transform.rotation * Quaternion.Euler(0, -90 - (_shoutEffect.shape.arc / 2), 0);
            _shout = Instantiate(_shoutEffect, shoutPos, shoutRot);
            _shout.Play();

            yield return new WaitForSeconds(.5f);

            var objects = Physics.OverlapSphere(transform.position, _attackDistance);
            foreach (var obj in objects)
            {
                var posTo = (obj.transform.position - transform.position).normalized;
                var dot = Vector3.Dot(posTo, transform.forward);
                if (dot >= Mathf.Cos(_shoutEffect.shape.arc / 2 * Mathf.Deg2Rad))
                {
                    var unit = obj.GetComponent<Unit>();
                    if (obj.CompareTag("Player") && unit != null && unit.enabled)
                    {
                        unit.UnitHealth.TakeDamage(_damage);
                        unit.UnitMovement.AddImpulse((_target.transform.position - transform.position) * _knockback, false);
                    }
                }
            }

            yield return new WaitForSeconds(_attackTime);

            _shout.Stop();
            StartCoroutine(DestoyTimed(.5f, _shout.gameObject));
            _shout = null;

            _unit.UnitAttack.InAttack = false;
            _unit.Animator.SetBool("Shout", false);

            _target = null;
        }

        private IEnumerator DestoyTimed(float time, GameObject obj)
        {
            yield return new WaitForSeconds(time);
            Destroy(obj);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _attackDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _damageDistance);
        }
    }
}

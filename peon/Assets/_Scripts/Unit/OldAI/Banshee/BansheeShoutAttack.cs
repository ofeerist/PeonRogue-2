using System.Collections;
using UnityEngine;

namespace _Scripts.Unit.AI.Banshee
{
    class BansheeShoutAttack : MonoCached.MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

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

        private Coroutine _shoutCoroutine;

        private AudioSource _audioSource;
        [SerializeField] private AudioClip _shoutClip;
        private static readonly int Shout1 = Animator.StringToHash("Shout");

        public void SetData(float attackDistance, float prepareTime, float attackTime, float attackCooldown, float damage, float knockback)
        {
            _attackDistance = attackDistance;
            _prepareTime = prepareTime;
            _attackTime = attackTime;
            _attackCooldown = attackCooldown;
            _damage = damage;
            _knockback = knockback;
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _attackCooldownTimer = Time.time;

            _audioSource = GetComponent<AudioSource>();

            AddFixedUpdate();

            InvokeRepeating(nameof(FindToAttack), 1f, .5f);
        }

        private void FindToAttack()
        {
            if (!_unit.enabled) return;

            var _transform = transform;

            if (_attackCooldownTimer <= Time.time)
            {
                var results = new Collider[10];
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _attackDistance, results, _layerMask);

                Collider closest = null;
                var minDistance = Mathf.Infinity;
                for (int i = 0; i < size; i++)
                {
                    if (Vector3.Distance(_transform.position, results[i].transform.position) < minDistance)
                    {
                        closest = results[i];
                    }
                }

                if (closest != null) _shoutCoroutine = StartCoroutine(Shout(closest.GetComponent<Unit>()));
            }
        }

        public void StopShout()
        {
            if (_shout == null) return;

            StopCoroutine(_shoutCoroutine);

            _shout.Stop();
            StartCoroutine(DestoyTimed(.5f, _shout.gameObject));
            _shout = null;
        }

        protected override void OnTick()
        {
            if (_unit)
            {
                if (_unit.UnitAttack.InAttack) return;
                if (!_unit.enabled)
                {
                    if (_shout)
                        StopShout();
                }
            }
        }

        protected override void OnFixedTick()
        {
            if (_shout) return;

            var _transform = transform;
            if (_target)
            {
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_target.transform.position - _transform.position), _rotateSpeed * Time.fixedDeltaTime);
            }
        }

        private IEnumerator Shout(Unit closest)
        {
            _target = closest;
            _attackCooldownTimer = Time.time + _attackCooldown;

            _unit.UnitAttack.InAttack = true;

            var _transform = transform;
            var position = _transform.position;
            var rotation = _transform.rotation;
            var eff = Instantiate(_prepareEffect, position, rotation);
            eff.Play();

            yield return new WaitForSeconds(_prepareTime);

            eff.Stop();
            StartCoroutine(DestoyTimed(.5f, eff.gameObject));

            _unit.Animator.SetBool(Shout1, true);
            _audioSource.PlayOneShot(_shoutClip);

            var shoutPos = position + new Vector3(0, .5f, 0) + _transform.forward;
            var shoutRot = rotation * Quaternion.Euler(0, -90 - (_shoutEffect.shape.arc / 2), 0);
            _shout = Instantiate(_shoutEffect, shoutPos, shoutRot);
            _shout.Play();

            yield return new WaitForSeconds(.5f);

            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(position, _attackDistance, results, _layerMask);
            for (int i = 0; i < size; i++)
            {
                var posTo = (results[i].transform.position - transform.position).normalized;
                var dot = Vector3.Dot(posTo, _transform.forward);
                if (dot >= Mathf.Cos(_shoutEffect.shape.arc / 2 * Mathf.Deg2Rad))
                {
                    var unit = results[i].GetComponent<Unit>();
                    if (unit && unit.enabled)
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
            _unit.Animator.SetBool(Shout1, false);

            _target = null;
        }

        private IEnumerator DestoyTimed(float time, GameObject obj)
        {
            yield return new WaitForSeconds(time);
            Destroy(obj);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.cyan;
            var position = transform.position;
            Gizmos.DrawWireSphere(position, _attackDistance);

            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireSphere(position, _damageDistance);
        }

        private void OnDestroy() => RemoveFixedUpdate();
        private void OnDisable() => RemoveFixedUpdate();
    }
}

using UnityEngine;
using System.Collections;

namespace Game.Unit
{
    class NecromancerProjectile : MonoCached
    {
        private float _damage;
        private float _knockback;
        private float _speed;
        private float _maxDistance;
        private float _damageRange;

        private Vector3 _startPosition;

        private ParticleSystem _disposeEffect;
        public static NecromancerProjectile Create(Vector3 position, Quaternion rotation, float speed, float maxDistance, float damage, float knockback, float damageRange, ParticleSystem disposeEffect)
        {
            var prj = Instantiate(Resources.Load<NecromancerProjectile>(nameof(NecromancerProjectile)), position, rotation);

            prj._damage = damage;
            prj._knockback = knockback;
            prj._speed = speed;
            prj._maxDistance = maxDistance;
            prj._damageRange = damageRange;

            prj._startPosition = position;

            prj._disposeEffect = disposeEffect;
            prj.AddFixedUpdate();

            return prj;
        }

        private ParticleSystem _destroy = null;

        private void Update()
        {
            if (_destroy != null) return;

            var objects = Physics.OverlapSphere(transform.position, _damageRange);
            foreach (var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (obj.CompareTag("Player") && unit.enabled && unit.UnitHealth != null)
                {
                    unit.UnitHealth.TakeDamage(_damage);

                    var posTo = (unit.transform.position - transform.position).normalized;
                    posTo.y = 0;
                    unit.UnitMovement.AddImpulse(posTo * _knockback, true, .3f);

                    DestroyPrj();
                }

                if(obj.gameObject.layer == 9) DestroyPrj();
            }
        }

        protected override void OnFixedTick()
        {
            if (_destroy != null) return;

            var transform = gameObject.transform;

            if (Vector3.Distance(_startPosition, transform.position) <= _maxDistance)
                transform.Translate(_speed * Time.deltaTime * transform.forward, Space.World);
            else
                DestroyPrj();
        }

        private void DestroyPrj()
        {
            if (_destroy != null) return;

            GetComponent<ParticleSystem>().Stop();

            _destroy = Instantiate(_disposeEffect);
            _destroy.transform.position = transform.position;
            _destroy.Play();

            StartCoroutine(DestroyEffectTimed(_destroy, .3f));
        }

        private IEnumerator DestroyEffectTimed(ParticleSystem ps, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(ps.gameObject);
            Destroy(gameObject);
        }

        private void OnDestroy() => RemoveFixedUpdate();
        private void OnDisable() => RemoveFixedUpdate();
    }
}

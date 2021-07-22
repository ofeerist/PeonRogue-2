using UnityEngine;
using Photon.Pun;
using System.Collections;

namespace Game.Unit
{
    class Axe : MonoCached
    { 
        private float _damage;
        private float _knockback;
        private float _speed;
        private float _maxDistance;
        private bool _trail;

        private Collider _collider;

        private Vector3 _startPosition;

        private ParticleSystem _disposeEffect;

        private AudioSource _audioSource;
        private AudioClip[] _hit;
        public static Axe Create(Collider ignore, Vector3 position, Quaternion rotation, float speed, float maxDistance, float damage, float knockback, ParticleSystem disposeEffect, AudioClip[] hit, bool trail = false)
        {
            var gameObject = Instantiate(Resources.Load<GameObject>("pickaxe"), position, rotation);
            var axe = gameObject.GetComponent<Axe>();

            axe._damage = damage;
            axe._knockback = knockback;
            axe._speed = speed;
            axe._maxDistance = maxDistance;

            axe._startPosition = position;

            axe._disposeEffect = disposeEffect;

            axe._collider = axe.GetComponent<Collider>();

            Physics.IgnoreCollision(ignore, axe._collider);

            axe._collider.enabled = true;

            axe._audioSource = axe.GetComponent<AudioSource>();
            axe._audioSource.Play();
            axe._hit = hit;

            axe._trail = trail;
            if (axe._trail)
            {
                var ps = axe.transform.Find("trail").GetComponent<ParticleSystem>();
                ps.gameObject.SetActive(true);
                ps.Play();
            }

            axe.AddFixedUpdate();

            return axe;
        }

        protected override void OnFixedTick()
        {
            var transform = gameObject.transform;

            if (Vector3.Distance(_startPosition, transform.position) <= _maxDistance)
                transform.Translate(_speed * Time.deltaTime * transform.forward, Space.World);
            else
                DestroyAxe();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!transform.GetChild(0).gameObject.activeSelf) return;
            Physics.IgnoreCollision(collision.collider, _collider);

            var unit = collision.gameObject.GetComponent<Unit>();

            if (unit != null && !unit.CompareTag("Player") && unit.UnitHealth != null && unit.enabled)
            {
                unit.UnitHealth.TakeDamage(_damage);

                _audioSource.Stop();
                _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);

                var posTo = (unit.transform.position - transform.position).normalized;
                posTo.y = 0;
                unit.UnitMovement.AddImpulse(posTo * _knockback);
            }

            if (!_trail)
                DestroyAxe();
        }

        private void DestroyAxe()
        {
            if (!transform.GetChild(0).gameObject.activeSelf) return;

            transform.GetChild(0).gameObject.SetActive(false);

            var ps = Instantiate(_disposeEffect);
            ps.transform.position = transform.position;
            ps.Play();

            StartCoroutine(DestroyEffectTimed(ps, 1));
        }

        private IEnumerator DestroyEffectTimed(ParticleSystem ps, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(ps.gameObject);
            Destroy(gameObject);
        }

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            RemoveFixedUpdate();
        }

        private void OnDestroy() => RemoveFixedUpdate();
    }
}

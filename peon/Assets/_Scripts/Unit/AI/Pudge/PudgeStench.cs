using System;
using System.Collections;
using UnityEngine;

namespace Game.Unit
{
    class PudgeStench : MonoCached
    {
        [SerializeField] private float _stenchUseRange;

        [SerializeField] private float _stenchRange;
        [SerializeField] private float _stenchDamage;
        [SerializeField] private float _stenchUsingTime;
        [SerializeField] private float _stenchUpdateDelay;
        private float _stenchUpdateDelayTimer;

        [SerializeField] private ParticleSystem _stenchEffect;
        [SerializeField] private ParticleSystem _stenchMarker;

        [SerializeField] private float _stenchPrepareTime;

        [SerializeField] private float _stenchCooldown;
        private float _stenchCooldownTimer;

        [Space]

        [SerializeField] private AudioSource _sound;

        private Unit _unit;
        private void Start()
        {
            _stenchCooldownTimer = Time.time;
            _stenchUpdateDelayTimer = Time.time;

            _unit = GetComponent<Unit>();
        }

        protected override void OnTick()
        {
            if (!_unit.enabled) return;

            if (_stenchCooldownTimer <= Time.time)
            {
                var objects = Physics.OverlapSphere(transform.position, _stenchUseRange);
                foreach (var obj in objects)
                {
                    if (obj.CompareTag("Player") && obj.GetComponent<Unit>().enabled)
                    {
                        _stenchCooldownTimer = Time.time + _stenchCooldown;
                        StartCoroutine(UseStench());
                        break;
                    }
                }
            }

            if (_stenchEffect.isPlaying)
            {
                if(_stenchUpdateDelayTimer <= Time.time)
                {
                    _stenchUpdateDelayTimer = Time.time + _stenchUpdateDelay;

                    var objects = Physics.OverlapSphere(transform.position, _stenchRange);
                    foreach (var obj in objects)
                    {
                        var unit = obj.GetComponent<Unit>();
                        if (obj.CompareTag("Player") && unit.enabled)
                        {
                            unit.UnitHealth.TakeDamage(_stenchDamage);
                        }
                    }
                }
            }
        }

        private IEnumerator UseStench()
        {
            _stenchMarker.Play();

            yield return new WaitForSeconds(_stenchPrepareTime);

            _stenchMarker.Stop();
            _stenchEffect.Play();
            _sound.Play();

            yield return new WaitForSeconds(_stenchUsingTime);

            _stenchEffect.Stop();
            _sound.Stop();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _stenchRange);
            Gizmos.DrawWireSphere(transform.position, _stenchUseRange);
        }
    }
}

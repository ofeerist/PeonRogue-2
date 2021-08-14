﻿using System.Collections;
using UnityEngine;

namespace _Scripts.Unit.AI.Pudge
{
    class PudgeStench : MonoCached.MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

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

        public void SetData(float useRange, float range, float damage, float usingTime, float cooldown)
        {
            _stenchUseRange = useRange;
            _stenchRange = range;
            _stenchDamage = damage;
            _stenchUsingTime = usingTime;
            _stenchCooldown = cooldown;
        }

        private void Start()
        {
            _stenchCooldownTimer = Time.time;
            _stenchUpdateDelayTimer = Time.time;

            _unit = GetComponent<Unit>();

            InvokeRepeating(nameof(FindTarget), 1, .5f);
        }

        private void FindTarget()
        {
            if (!_unit.enabled) return;

            if (_stenchCooldownTimer <= Time.time)
            {
                var objects = Physics.OverlapSphere(transform.position, _stenchUseRange, _layerMask);
                foreach (var obj in objects)
                {
                    if (obj.GetComponent<Unit>().enabled)
                    {
                        _stenchCooldownTimer = Time.time + _stenchCooldown;
                        StartCoroutine(UseStench());
                        break;
                    }
                }
            }
        }

        protected override void OnTick()
        {
            if (!_unit.enabled) return;

            if (_stenchEffect.isPlaying)
            {
                if(_stenchUpdateDelayTimer <= Time.time)
                {
                    _stenchUpdateDelayTimer = Time.time + _stenchUpdateDelay;

                    var objects = Physics.OverlapSphere(transform.position, _stenchRange, _layerMask);
                    foreach (var obj in objects)
                    {
                        var unit = obj.GetComponent<Unit>();
                        if (unit.enabled)
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
            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _stenchRange);
            Gizmos.DrawWireSphere(transform.position, _stenchUseRange);
        }
    }
}
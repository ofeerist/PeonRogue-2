﻿using System.Collections;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace _Scripts.Unit.AI
{
    class EnemyHealth : UnitHealth
    {
        [SerializeField] private float _stanTime;
        [SerializeField] private ParticleSystem _stanChannelEffect;
        [SerializeField] private ParticleSystem _stanFinishlEffect;
        private float _currentStanTime;

        [Space]

        private TextTag.TextTag _textTag;
        [SerializeField] private Vector3 _textTagVelocity;
        [SerializeField] private float _textTagLifetime;
        [SerializeField] private float _textTagColorizingTime;

        [Space]

        [SerializeField] private Slider _healthSlider;

        [Space]

        [SerializeField] private AudioSource _onHit;
        [SerializeField] private AudioClip[] _onHitSounds;
        [SerializeField] private AudioSource _onDeath;
        private static readonly int Dead1 = Animator.StringToHash("Dead");

        public override event Dead OnDeath;

        public void SetData(float maxHealth, float stanTime)
        {
            MaxHealth = maxHealth;
            _stanTime = stanTime;
        }

        [PunRPC]
        public override void TakeDamage(float damage)
        {
            if (damage == 0) return;

            _currentHealth -= damage;
            _healthSlider.value = _currentHealth / MaxHealth;
            SetTextTag(damage);

            _onHit.clip = _onHitSounds[Random.Range(0, _onHitSounds.Length)];
            _onHit.Play();

            if (_currentHealth <= 0) Death();
            else Stan(_stanTime);
            
        }

        public override void Stan(float time)
        {
            if (!Unit.enabled) return;

            _stanTime = time;
            _currentStanTime = Time.time + time;
            InStan = true;
            
            Observable.FromCoroutine(Stagger).Subscribe().AddTo(this);
        }

        private void SetTextTag(float damage)
        {
            var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
            if (_textTag == null)
            {
                _textTag = TextTag.TextTag.Create(transform.position + randomOffset, Mathf.RoundToInt(damage).ToString(), UnityEngine.Color.blue, _textTagLifetime, _textTagVelocity, false);
            }
            else
            {
                _textTag.transform.position = transform.position + randomOffset;
                _textTag.Text = _textTag.Color.a >= .2f ? (System.Convert.ToInt32(_textTag.Text) + Mathf.RoundToInt(damage)).ToString() : Mathf.RoundToInt(damage).ToString();

                _textTag.Color = UnityEngine.Color.Lerp(_textTag.Color, UnityEngine.Color.red, _textTagColorizingTime * Time.deltaTime) + new UnityEngine.Color(0, 0, 0, 1);
                _textTag.LifeTime = _textTagLifetime;
            }
        }

        private void Death()
        {
            _onDeath.Play();
            Unit.GetComponent<NavMeshAgent>().enabled = false;
            Unit.UnitMovement.enabled = false;
            
            Observable.FromCoroutine(SetKinematic).Subscribe().AddTo(this);
            
            _healthSlider.gameObject.SetActive(false);

            Unit.enabled = false;
            _stanChannelEffect.Stop();
            Unit.Animator.speed = 1;
            Unit.Animator.SetBool(Dead1, true);

            OnDeath?.Invoke(Unit);

            Observable.FromCoroutine(DeathDispose).Subscribe().AddTo(this);
        }

        private IEnumerator DeathDispose()
        {
            yield return new WaitForSeconds(2f);
            
            int k = 0;
            var _transform = transform;
            while (++k <= 500)
            {
                yield return null;
                _transform.position -= new Vector3(0, .3f * Time.deltaTime, 0);
            }
            Destroy(gameObject);
        }
        
        private IEnumerator SetKinematic()
        {
            yield return null;
            Unit.Rigidbody.isKinematic = true;
            var colliders = Unit.GetComponents<Collider>();
            foreach (var item in colliders)
            {
                item.enabled = false;
            }
        }

        private IEnumerator Stagger()
        {
            Unit.Animator.speed = 0;
            Unit.UnitMovement.BlockMovement = true;

            _stanChannelEffect.Play();

            var stanTimed = _currentStanTime;
            
            yield return new WaitForSeconds(_stanTime);

            if(_currentStanTime == stanTimed && Unit.enabled)
            {
                Unit.Animator.speed = 1;
                Unit.UnitMovement.BlockMovement = false;
                InStan = false;

                _stanChannelEffect.Stop();
                _stanFinishlEffect.Play();
            }
        }
    }
}
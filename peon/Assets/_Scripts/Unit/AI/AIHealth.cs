using System;
using System.Collections;
using _Scripts.Unit.Player;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI
{
    public class AIHealth : MonoBehaviour
    {
        private float _stanTime;
        
        public float BounceDamage { get; private set; }

        [SerializeField] private float _maxHealth;
        private float _currentHealth;
        
        
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
        
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        
        public AIHealth(float bounceDamage)
        {
            BounceDamage = bounceDamage;
        }

        public delegate void Dead(Unit u);
        public virtual event Dead OnDeath;

        private Unit _unit;
        private KinematicCharacterMotor _motor;
        private MovementAI _movement;

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _motor = GetComponent<KinematicCharacterMotor>();
            _movement = GetComponent<MovementAI>();
            
            _currentHealth = _maxHealth;
        }

        public void SetData(float maxHealth)
        {
            _maxHealth = maxHealth;
        }

        [PunRPC]
        public void AddVelocity(Vector3 velocity)
        {
            _movement.AddVelocity(velocity);
        }
        
        [PunRPC]
        public void TakeDamage(float damage, float bounceDamage, float stanTime)
        {
            if (damage == 0) return;

            BounceDamage = bounceDamage;
            _stanTime = stanTime;
            
            _currentHealth -= damage;
            _healthSlider.value = _currentHealth / _maxHealth;
            SetTextTag(damage);

            _onHit.clip = _onHitSounds[new System.Random((int)damage).Next(0, _onHitSounds.Length)];
            _onHit.Play();

            if (_currentHealth <= 0) Death();
            else Stan(_stanTime);
        }

        public void Stan(float time)
        {
            if (!_unit.enabled) return;

            _unit.CurrentState = UnitState.InStan;
            _unit.Animator.speed = 0;
            
            _stanTime = time;
            var stanTime = _currentStanTime = Time.time + time;

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(x =>
            {
                if (Math.Abs(_currentStanTime - stanTime) < .01)
                {
                    _unit.CurrentState = UnitState.Default;
                    _unit.Animator.speed = 1;
                }
            });
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
            _unit.Animator.SetBool(Dead1, true);
            _onDeath.Play();
            _unit.CurrentState = UnitState.Dead;
            
            _healthSlider.gameObject.SetActive(false);

            _unit.enabled = false;
            _motor.enabled = false;
            
            _stanChannelEffect.Stop();

            OnDeath?.Invoke(_unit);

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                _serialDisposable.Disposable = Observable.FromMicroCoroutine(Dispose).Subscribe();
            });
        }

        private IEnumerator Dispose()
        {
            var _transform = transform;
            int k = 0;
            while(++k < 500)
            {
                yield return null;
                _transform.position -= new Vector3(0, .3f * Time.deltaTime, 0);
            }
            
            Destroy(gameObject); 
        }
    }
}
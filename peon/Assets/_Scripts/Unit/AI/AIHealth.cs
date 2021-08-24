using System;
using System.Collections;
using _Scripts.Unit.Doodads;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI
{
    public sealed class AIHealth : MonoBehaviour
    {
        private float _stanTime;

        private float _bounceDamage;

        [SerializeField] private float _maxHealth;
        private float _currentHealth;
        
        [SerializeField] private bool _doodad;

        [SerializeField] private int _goldReward;
        [SerializeField] private GoldProjectile _goldPrefab;
        
        [Space] [SerializeField] private float _knockbackDamageCooldown;
        private float _knockbackTimer;
        
        [Space]
        
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

        public delegate void Dead(Unit u);
        public event Dead OnDeath;

        private Unit _unit;
        private KinematicCharacterMotor _motor;
        private MovementAI _movement;
        private PhotonView _photonView;

        private int _lastID;
        private static readonly int Hit = Animator.StringToHash("Hit");

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _motor = GetComponent<KinematicCharacterMotor>();
            _movement = GetComponent<MovementAI>();
            _photonView = GetComponent<PhotonView>();
            
            if(_movement != null) _movement.ByMovementHit += ByMovementHit;
            
            _currentHealth = _maxHealth;

            _bounceDamage = 0;
        }

        private void ByMovementHit(Collider hitcollider, Vector3 hitnormal, Vector3 hitpoint)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (_knockbackTimer > Time.time) return;
            
            var layer = hitcollider.gameObject.layer;
            if (layer != 9) return;

            _knockbackTimer = _knockbackDamageCooldown + Time.time;
            _photonView.RPC(nameof(TakeDamage), RpcTarget.AllViaServer, _bounceDamage);
        }

        public void SetData(float maxHealth)
        {
            _maxHealth = maxHealth;
        }

        [PunRPC]
        public void AddVelocity(float x, float y, float z)
        {
            if(!_doodad) _movement.AddVelocity(new Vector3(x, y, z));
        }
        
        [PunRPC]
        public void TakeDamage(float damage)
        {
            if (damage == 0) return;

            _currentHealth -= damage;
            _healthSlider.value = _currentHealth / _maxHealth;
            SetTextTag(damage);

            _onHit.clip = _onHitSounds[new System.Random((int)damage).Next(0, _onHitSounds.Length)];
            _onHit.Play();

            if (_currentHealth <= 0) Death(_lastID);
            else Stan(_stanTime);
        }
        
        [PunRPC]
        public void TakeDamage(float damage, float bounceDamage, float stanTime, int killerID)
        {
            if (damage == 0) return;
            if (_currentHealth <= 0) return;
            
            if(bounceDamage != 0) _bounceDamage = bounceDamage;
            if(stanTime != 0) _stanTime = stanTime;
            
            _currentHealth -= damage;
            
            switch (_doodad)
            {
                case true:
                    _unit.Animator.SetTrigger(Hit);
                    break;
                case false:
                    _healthSlider.value = _currentHealth / _maxHealth;
                    SetTextTag(damage);
                
                    _onHit.clip = _onHitSounds[new System.Random((int)damage).Next(0, _onHitSounds.Length)];
                    _onHit.Play();
                    break;
            }

            _lastID = killerID;
            if (_currentHealth <= 0) Death(killerID);
            else if(!_doodad) Stan(_stanTime);
        }

        private void Stan(float time)
        {
            _unit.CurrentState = UnitState.InStan;
            _unit.Animator.speed = 0;
            _stanChannelEffect.Play();
            
            _stanTime = time;
            var stanTime = _currentStanTime = Time.time + time;

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(x =>
            {
                if (Math.Abs(_currentStanTime - stanTime) < .01)
                {
                    _unit.CurrentState = UnitState.Default;
                    _unit.Animator.speed = 1;

                    _stanChannelEffect.Stop();
                    _stanFinishlEffect.Play();
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
                _textTag.Text = _textTag.Color.a >= .2f ? (Convert.ToInt32(_textTag.Text) + Mathf.RoundToInt(damage)).ToString() : Mathf.RoundToInt(damage).ToString();

                _textTag.Color = UnityEngine.Color.Lerp(_textTag.Color, UnityEngine.Color.red, _textTagColorizingTime * Time.deltaTime) + new UnityEngine.Color(0, 0, 0, 1);
                _textTag.LifeTime = _textTagLifetime;
            }
        }

        private void Death(int killerID)
        {
            _unit.Animator.SetBool(Dead1, true);
            _onDeath.Play();
            _unit.CurrentState = UnitState.Dead;

            if (!_doodad)
            {
                _healthSlider.gameObject.SetActive(false);

                _unit.enabled = false;
                _motor.Capsule.enabled = false;
                _motor.enabled = false;

                _stanChannelEffect.Stop();
            }
            else
            {
                GetComponent<CapsuleCollider>().enabled = false;
            }
            
            CreateGoldReward(PhotonView.Find(killerID).GetComponent<Unit>());
            
            OnDeath?.Invoke(_unit);

            if(_doodad) return;

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                _serialDisposable.Disposable = Observable.FromMicroCoroutine(Dispose).Subscribe(z =>
                {
                    Destroy(gameObject);
                });
            });
        }

        private void CreateGoldReward(Unit unit)
        {
            GoldProjectile.Create(_goldPrefab, transform.position + new Vector3(0, .5f, 0), unit, _goldReward);
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
        }
    }
}
using System;
using _Scripts.Unit.Player;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.AI.Pudge
{
    public class StenchPudge : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _stenchUseRange;

        [SerializeField] private float _stenchRange;
        [SerializeField] private float _stenchDamage;
        [SerializeField] private float _stenchUsingTime;
        [SerializeField] private float _stenchUpdateDelay;

        [SerializeField] private ParticleSystem _stenchEffect;
        [SerializeField] private ParticleSystem _stenchMarker;

        [SerializeField] private float _stenchPrepareTime;

        [SerializeField] private float _stenchCooldown;
        private float _stenchCooldownTimer;

        [Space]

        [SerializeField] private AudioSource _sound;

        private bool _isStench;
        
        private readonly Collider[] _results = new Collider[6];
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        private PhotonView _photonView;

        public void SetData(float useRange, float range, float damage, float usingTime, float cooldown)
        {
            _stenchUseRange = useRange;
            _stenchRange = range;
            _stenchDamage = damage;
            _stenchUsingTime = usingTime;
            _stenchCooldown = cooldown;
        }

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _stenchCooldownTimer = Time.time;
            
            _photonView = GetComponent<PhotonView>();
            
            if (!PhotonNetwork.IsMasterClient) return;
            
            // Find Target
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                if (_isStench || _stenchCooldownTimer > Time.time) return;
                
                var size = Physics.OverlapSphereNonAlloc(transform.position, _stenchUseRange, _results, _layerMask);
                if (size > 0)
                {
                    _stenchCooldownTimer = Time.time + _stenchCooldown;
                    _isStench = true;
                    _photonView.RPC(nameof(UseStench), RpcTarget.AllViaServer);
                }

            }).AddTo(this);
            
            // Damage
            Observable.Interval(TimeSpan.FromSeconds(_stenchUpdateDelay)).Subscribe(x =>
            {
                if (!_isStench) return;
                
                var size = Physics.OverlapSphereNonAlloc(transform.position, _stenchRange, _results, _layerMask);
                for(int i = 0; i < size; i++)
                {
                    var unit = _results[i].GetComponent<Unit>();
                    if (unit.enabled)
                    {
                        unit.PhotonView.RPC(nameof(PlayerHealth.TakeDamage), RpcTarget.AllViaServer, _stenchDamage);
                    }
                }
            }).AddTo(this);
        }


        [PunRPC]
        private void UseStench()
        {
            _stenchMarker.Play();
            
            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_stenchPrepareTime)).Subscribe(z =>
            {
                
                _stenchMarker.Stop();
                _stenchEffect.Play();
                _sound.Play();
                
                _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(_stenchUsingTime)).Subscribe(x =>
                {
                    _stenchEffect.Stop();
                    _sound.Stop();
                });
            });
        }
    }
}
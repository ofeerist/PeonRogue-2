using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using _Scripts.Unit.AI;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.Player
{
    public class Axe : MonoBehaviour
    { 
        private float _damage;
        private float _knockback;
        private float _speed;
        private float _maxDistance;
        private bool _trail;
        
        private Vector3 _startPosition;

        private readonly Collider[] _results = new Collider[1];
        
        [SerializeField] private float _detectRadius;
        
        [SerializeField] private LayerMask _obstacle;
        [SerializeField] private LayerMask _enemy;
        
        [SerializeField] private ParticleSystem _disposeEffect;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _hit;
        
        [SerializeField] private ParticleSystem _trailParticle;

        private Unit _creator;
        private readonly List<Unit> _ignore = new List<Unit>();
        private PhotonView _photonView;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.DrawWireSphere(transform.position, _detectRadius);
        }

        // Создавать должен только мастер клиент
        public static Axe Create(Axe prefab, Unit creator, Vector3 position, Quaternion rotation, float speed, float maxDistance, float damage, float knockback, bool trail = false)
        {
            var pp = PhotonNetwork.Instantiate(prefab.name, position, rotation);
            var axe = pp.GetComponent<Axe>();
            axe._creator = creator;
            axe._photonView = axe.GetComponent<PhotonView>();
            
            axe._damage = damage;
            axe._knockback = knockback;
            axe._speed = speed;
            axe._maxDistance = maxDistance;

            axe._startPosition = position;
            
            axe._audioSource.Play();

            axe._trail = trail;
            if (axe._trail)
            {
                axe._trailParticle.gameObject.SetActive(true);
                axe._trailParticle.Play();
            }
            
            axe.Begin();
            
            return axe;
        }

        private void Begin()
        {
            var _transform = transform;
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (!transform.GetChild(0).gameObject.activeSelf) return;
                
                DetectCollision(_transform);

                Move(_transform);
            }).AddTo(this);
            
            Observable.Timer(TimeSpan.FromSeconds(10f)).Subscribe(x =>
            {
                if (!transform.GetChild(0).gameObject.activeSelf) return;
                
                Destroy(gameObject);
            }).AddTo(this);
        }

        private void DetectCollision(Transform _transform)
        {
            // Check for obstacles
            var size = Physics.OverlapSphereNonAlloc(_transform.position, _detectRadius, _results, _obstacle);
            if (size > 0)
            {
                if (!_trail)
                    DestroyAxe();
            }
            
            // Check for enemies
            size = Physics.OverlapSphereNonAlloc(_transform.position, _detectRadius, _results, _enemy);
            if (size > 0)
            {
                var unit = _results[0].gameObject.GetComponent<Unit>();

                if (unit != null && unit.enabled && !_ignore.Contains(unit))
                {
                    _ignore.Add(unit);
                
                    _audioSource.Stop();
                    _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);

                    unit.PhotonView.RPC(nameof(AIHealth.TakeDamage), RpcTarget.AllViaServer,
                        _damage, _creator.BounceDamage, _creator.TimeToStan, _photonView.ViewID);

                    var posTo = (unit.transform.position - transform.position).normalized;
                    posTo.y = 0;
                    posTo *= _knockback;
                    unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                        posTo.x, posTo.y, posTo.z);
                }

                if (!_trail)
                    DestroyAxe();
            }
        }

        private void Move(Transform _transform)
        {
            if (Vector3.Distance(_startPosition, _transform.position) <= _maxDistance)
                _transform.Translate(_speed * Time.deltaTime * _transform.forward, Space.World);
            else
                DestroyAxe();
        }

        private void DestroyAxe()
        {
            if (!transform.GetChild(0).gameObject.activeSelf) return;

            transform.GetChild(0).gameObject.SetActive(false);

            _photonView.RPC(nameof(End), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void End()
        {
            var ps = Instantiate(_disposeEffect);
            ps.transform.position = transform.position;
            ps.Play();

            Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                Destroy(ps.gameObject);
                Destroy(gameObject);
            }).AddTo(this);
        }
    }
}

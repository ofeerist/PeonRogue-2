using System;
using System.Collections;
using _Scripts.Unit.Player;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.AI.Necromancer
{
    class NecromancerProjectile : MonoCached.MonoCached
    { 
        private float _damage;
        private float _knockback;
        private float _speed;
        private float _maxDistance;

        private Vector3 _startPosition;

        private readonly Collider[] _results = new Collider[1];
        
        [SerializeField] private float _detectRadius;
        
        [SerializeField] private LayerMask _obstacle;
        [SerializeField] private LayerMask _enemy;
        
        [SerializeField] private ParticleSystem _disposeEffect;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _hit;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.green;
            Gizmos.DrawWireSphere(transform.position, _detectRadius);
        }

        // Создавать должен только мастер клиент
        public static NecromancerProjectile Create(NecromancerProjectile prefab, Vector3 position, Quaternion rotation, float speed, float maxDistance, float damage, float knockback)
        {
            var pp = PhotonNetwork.Instantiate(prefab.name, position, rotation);
            var prj = pp.GetComponent<NecromancerProjectile>();

            prj._damage = damage;
            prj._knockback = knockback;
            prj._speed = speed;
            prj._maxDistance = maxDistance;

            prj._startPosition = position;
            
            prj._audioSource.Play();

            prj.Begin();
            
            return prj;
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
        }

        private void DetectCollision(Transform _transform)
        {
            // Check for obstacles
            var size = Physics.OverlapSphereNonAlloc(_transform.position, _detectRadius, _results, _obstacle);
            if (size > 0)
            {
                DestroyAxe();
            }
            
            // Check for enemies
            size = Physics.OverlapSphereNonAlloc(_transform.position, _detectRadius, _results, _enemy);
            if (size > 0)
            {
                var unit = _results[0].gameObject.GetComponent<Unit>();

                if (unit != null && unit.enabled)
                {
                    _audioSource.Stop();
                    _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);

                    unit.PhotonView.RPC(nameof(PlayerHealth.TakeDamage), RpcTarget.AllViaServer,
                        _damage);

                    var posTo = (unit.transform.position - transform.position).normalized;
                    posTo.y = 0;
                    posTo *= _knockback;
                    unit.PhotonView.RPC(nameof(PlayerHealth.AddVelocity), RpcTarget.AllViaServer,
                        posTo.x, posTo.y, posTo.z);
                }
                
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

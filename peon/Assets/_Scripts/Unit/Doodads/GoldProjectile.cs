using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.Doodads
{
    public class GoldProjectile : MonoBehaviour
    {
        [SerializeField] private LayerMask _player;
        [SerializeField] private float _speed;

        private readonly Collider[] _results = new Collider[1];
        
        public static void Create(GoldProjectile prefab, Vector3 position, Unit target, int goldReward)
        {
            var p = Instantiate(prefab, position, Quaternion.identity);

            p.Proccess(target, goldReward);
        }

        private void Proccess(Unit target, int goldReward)
        {
            var _transform = transform;

            var toUnit = false;
            var dir = _transform.position + new Vector3(0, 2, 0);

            var ended = false;
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (ended) return;
                
                var position = _transform.position;
                position += ((toUnit ? target.transform.position : dir) - position) * _speed * Time.deltaTime;
                _transform.position = position;

                var size  = Physics.OverlapSphereNonAlloc(position, .1f, _results, _player);

                if (size > 0 && _results[0].gameObject.Equals(target.gameObject))
                {
                    ended = true;
                    
                    target.ReceiveGold(goldReward);
                    GetComponent<AudioSource>().Play();
                    Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(z =>
                    {
                        Destroy(gameObject);
                    }).AddTo(this);
                }

            }).AddTo(this);

            Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                toUnit = true;
            }).AddTo(this);
        }
    }
}
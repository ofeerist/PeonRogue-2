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
            
            Observable.EveryUpdate().Subscribe(x =>
            {
                var position = _transform.position;
                position += (target.transform.position - position) * _speed * Time.deltaTime;
                _transform.position = position;

                var size  = Physics.OverlapSphereNonAlloc(position, .1f, _results, _player);

                if (size > 0 && _results[0].gameObject.Equals(target.gameObject))
                {
                    target.ReceiveGold(goldReward);
                    Destroy(gameObject);
                }

            }).AddTo(this);
        }
    }
}
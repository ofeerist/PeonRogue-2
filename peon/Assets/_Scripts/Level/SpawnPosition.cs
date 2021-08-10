using UnityEngine;

namespace _Scripts.Level
{
    public class SpawnPosition : MonoBehaviour
    {
        [SerializeField] private float _range;

        public Vector3 GetPosition()
        {
            var _transform = transform;
            var position = _transform.position;
            float x = position.x + Random.Range(-_range, _range);
            float z = position.z + Random.Range(-_range, _range);
            return new Vector3(x, position.y, z);
        }
        
        public Vector3 GetPosition(System.Random r)
        {
            var _transform = transform;
            var position = _transform.position;
            float x = position.x + NextFloat(r,-_range, _range);
            float z = position.z + NextFloat(r,-_range, _range);
            return new Vector3(x, position.y, z);
        }
        
        private static float NextFloat(System.Random r, float min, float max){
            double val = (r.NextDouble() * (max - min) + min);
            return (float)val;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}

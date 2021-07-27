using UnityEngine;

namespace Game.Level
{
    class SpawnPosition : MonoBehaviour
    {
        [SerializeField] private float _range;

        private Vector3 GetPosition()
        {
            var _transform = transform;
            float x = _transform.position.x + Random.Range(-_range, _range);
            float z = _transform.position.z + Random.Range(-_range, _range);
            return new Vector3(x, _transform.position.y, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}

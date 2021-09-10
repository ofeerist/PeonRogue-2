using UnityEngine;

namespace _Scripts.Campaign.Ocean
{
    public class OceanObstacle : MonoBehaviour
    {
        [SerializeField] private LayerMask _conflictLayer;
        [SerializeField] private float _conflictRadius;

        private readonly Collider[] _results = new Collider[2];
        
        public bool Validate()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, _conflictRadius, _results, _conflictLayer);
            
            return size == 1;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _conflictRadius);
        }
    }
}
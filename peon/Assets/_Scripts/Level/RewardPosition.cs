using UnityEngine;

namespace Game.Level
{
    class RewardPosition : MonoBehaviour
    {
        [SerializeField] private float _angle;
        [SerializeField] private float _distance;

        public void CreateReward()
        {

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var _transform = transform;
            var k = 0;
            for (float i = 0; i < 360; i += _angle)
            {
                Gizmos.DrawWireSphere(_transform.position + Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * _distance, .1f);
                k++;
                if(k > 100) break; 
            }
        }
    }
}

using _Scripts.Unit.AI;
using UnityEngine;

namespace _Scripts.Level.UnitData
{
    [CreateAssetMenu(fileName = "New SkeletonData", menuName = "UnitData/Skeleton Data", order = 51)]
    class SkeletonData : UnitData
    {
        [SerializeField] private RandomFloat _maxHealth;

        [Space]

        [SerializeField] private bool _chase;
        [SerializeField] private RandomFloat _minDistanceToChase;
        [SerializeField] private RandomFloat _maxDistanceToChase;
        [SerializeField] private bool _retreat;
        [SerializeField] private RandomFloat _retreatDistance;
        
        [SerializeField] private RandomFloat _speed;


        [Space]

        [SerializeField] private RandomFloat _attackSpeed;
        [SerializeField] private RandomFloat _range;
        [SerializeField] private RandomFloat _angle;
        [SerializeField] private RandomFloat _damage;

        public override void SetData(Unit.Unit unit)
        {
            var health = unit.GetComponent<AIHealth>();
            var aiMovement = unit.GetComponent<MovementAI>();
            var aiAttack = unit.GetComponent<MeleeAIAttack>();

            health.SetData(_maxHealth.GetValue());
            aiMovement.SetData(_speed.GetValue(), _chase, _maxDistanceToChase.GetValue(), _minDistanceToChase.GetValue(), _retreat, _retreatDistance.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
        }
    }
}

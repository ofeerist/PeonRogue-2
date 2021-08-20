using _Scripts.Unit.AI;
using _Scripts.Unit.AI.Skeleton;
using UnityEngine;

namespace _Scripts.Level.UnitData
{
    [CreateAssetMenu(fileName = "New OrcSkeletonData", menuName = "UnitData/OrcSkeleton Data", order = 51)]
    class OrcSkeletonData : UnitData
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

        [Space]

        [SerializeField] private RandomFloat _dashMaxDetectRange;
        [SerializeField] private RandomFloat _dashMinDetectRange;

        [SerializeField] private RandomFloat _dashDamageRange;
        [SerializeField] private RandomFloat _dashDamage;

        [SerializeField] private RandomFloat _dashSpeed;

        [SerializeField] private RandomFloat _timeToPrepare;

        [SerializeField] private RandomFloat _dashCooldown;

        public override void SetData(Unit.Unit unit)
        {
            var health = unit.GetComponent<AIHealth>();
            var aiMovement = unit.GetComponent<MovementAI>();
            var aiAttack = unit.GetComponent<MeleeAIAttack>();
            var skeletonDash = unit.GetComponent<DashSkeleton>();

            health.SetData(_maxHealth.GetValue());
            aiMovement.SetData(_speed.GetValue(), _chase, _maxDistanceToChase.GetValue(), _minDistanceToChase.GetValue(), _retreat, _retreatDistance.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
            skeletonDash.SetData(_dashMaxDetectRange.GetValue(), _dashMinDetectRange.GetValue(), _dashDamageRange.GetValue(), _dashDamage.GetValue(), _dashSpeed.GetValue(), _timeToPrepare.GetValue(), _dashCooldown.GetValue());
        }
    }
}

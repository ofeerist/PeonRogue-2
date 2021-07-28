using UnityEngine;
using Game.Unit;

namespace Game.Level.UnitData
{
    [CreateAssetMenu(fileName = "New OrcSkeletonData", menuName = "UnitData/OrcSkeleton Data", order = 51)]
    class OrcSkeletonData : UnitData
    {
        [SerializeField] private RandomFloat _maxHealth;
        [SerializeField] private RandomFloat _stanTime;

        [Space]

        [SerializeField] private RandomFloat _detectionRange;
        [SerializeField] private RandomFloat _rotateSpeed;
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
            var health = unit.GetComponent<EnemyHealth>();
            var aiMovement = unit.GetComponent<AIMovement>();
            var aiAttack = unit.GetComponent<AIAttack>();
            var skeletonDash = unit.GetComponent<SkeletonDash>();

            health.SetData(_maxHealth.GetValue(), _stanTime.GetValue());
            aiMovement.SetData(_detectionRange.GetValue(), _rotateSpeed.GetValue(), _speed.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
            skeletonDash.SetData(_dashMaxDetectRange.GetValue(), _dashMinDetectRange.GetValue(), _dashDamageRange.GetValue(), _dashDamage.GetValue(), _dashSpeed.GetValue(), _timeToPrepare.GetValue(), _dashCooldown.GetValue());
        }
    }
}

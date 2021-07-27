using UnityEngine;
using Game.Unit;

namespace Game.Level.UnitData
{
    [CreateAssetMenu(fileName = "New BansheeData", menuName = "UnitData/Banshee Data", order = 51)]
    class BansheeData : UnitData
    {
        public static int UsageTimes;

        [SerializeField] private RandomFloat _maxHealth;
        [SerializeField] private RandomFloat _stanTime;

        [Space]

        [SerializeField] private RandomFloat _distanceToRetreat;
        [SerializeField] private RandomFloat _retreatRange;

        [SerializeField] private bool _chase;
        [SerializeField] private RandomFloat _minDistanceToChase;
        [SerializeField] private RandomFloat _maxDistanceToChase;
        [SerializeField] private RandomFloat _chaseRange;

        [SerializeField] private RandomFloat _teleportCooldown;

        [Space]

        [SerializeField] private RandomFloat _attackDistance;
        [SerializeField] private RandomFloat _prepareTime;
        [SerializeField] private RandomFloat _attackTime;
        [SerializeField] private RandomFloat _attackCooldown;
        [SerializeField] private RandomFloat _damage;
        [SerializeField] private RandomFloat _knockback;

        public override void SetData(Unit.Unit unit)
        {
            var health = unit.GetComponent<EnemyHealth>();
            var aiMovement = unit.GetComponent<BansheeMovement>();
            var aiAttack = unit.GetComponent<BansheeShoutAttack>();

            health.SetData(_maxHealth.GetValue(), _stanTime.GetValue());
            aiMovement.SetData(_distanceToRetreat.GetValue(), _retreatRange.GetValue(), _chase, _minDistanceToChase.GetValue(), _maxDistanceToChase.GetValue(), _chaseRange.GetValue(), _teleportCooldown.GetValue());
            aiAttack.SetData(_attackDistance.GetValue(), _prepareTime.GetValue(), _attackTime.GetValue(), _attackCooldown.GetValue(), _damage.GetValue(), _knockback.GetValue());
        }
    }
}

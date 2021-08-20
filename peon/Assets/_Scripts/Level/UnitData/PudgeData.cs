using _Scripts.Unit.AI;
using _Scripts.Unit.AI.Pudge;
using UnityEngine;

namespace _Scripts.Level.UnitData
{
    [CreateAssetMenu(fileName = "New PudgeData", menuName = "UnitData/Pudge Data", order = 51)]
    class PudgeData : UnitData
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

        [SerializeField] private RandomFloat _stenchUseRange;
        [SerializeField] private RandomFloat _stenchRange;
        [SerializeField] private RandomFloat _stenchDamage;
        [SerializeField] private RandomFloat _stenchUsingTime;
        [SerializeField] private RandomFloat _stenchCooldown;

        [Space]

        [SerializeField] private RandomFloat _maxHookDistance;
        [SerializeField] private RandomFloat _hookSpeed;
        [SerializeField] private RandomFloat _hookBackwardSpeed;
        [SerializeField] private RandomFloat _rangeToUseHook;
        [SerializeField] private RandomFloat _minRangeToUseHook;
        [SerializeField] private RandomFloat _hookCooldown;

        public override void SetData(Unit.Unit unit)
        {
            var health = unit.GetComponent<AIHealth>();
            var aiMovement = unit.GetComponent<MovementAI>();
            var aiAttack = unit.GetComponent<MeleeAIAttack>();
            var stench = unit.GetComponent<StenchPudge>();
            var hook = unit.GetComponent<HookPudge>();

            health.SetData(_maxHealth.GetValue());
            aiMovement.SetData(_speed.GetValue(), _chase, _maxDistanceToChase.GetValue(), _minDistanceToChase.GetValue(), _retreat, _retreatDistance.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
            stench.SetData(_stenchUseRange.GetValue(), _stenchRange.GetValue(), _stenchDamage.GetValue(), _stenchUsingTime.GetValue(), _stenchCooldown.GetValue());
            hook.SetData(_maxHookDistance.GetValue(), _hookSpeed.GetValue(), _hookBackwardSpeed.GetValue(), _rangeToUseHook.GetValue(), _minRangeToUseHook.GetValue(), _hookCooldown.GetValue()); 
        }
    }
}

using UnityEngine;
using Game.Unit;

namespace Game.Level.UnitData
{
    [CreateAssetMenu(fileName = "New PudgeData", menuName = "UnitData/Pudge Data", order = 51)]
    class PudgeData : UnitData
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
            var health = unit.UnitHealth as EnemyHealth;
            var aiMovement = unit.UnitMovement as AIMovement;
            var aiAttack = unit.UnitAttack as AIAttack;
            var stench = unit.GetComponent<PudgeStench>();
            var hook = unit.GetComponent<PudgeHook>();

            health.SetData(_maxHealth.GetValue(), _stanTime.GetValue());
            aiMovement.SetData(_detectionRange.GetValue(), _rotateSpeed.GetValue(), _speed.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
            stench.SetData(_stenchUseRange.GetValue(), _stenchRange.GetValue(), _stenchDamage.GetValue(), _stenchUsingTime.GetValue(), _stenchCooldown.GetValue());
            hook.SetData(_maxHookDistance.GetValue(), _hookSpeed.GetValue(), _hookBackwardSpeed.GetValue(), _rangeToUseHook.GetValue(), _minRangeToUseHook.GetValue(), _hookCooldown.GetValue()); 
        }
    }
}

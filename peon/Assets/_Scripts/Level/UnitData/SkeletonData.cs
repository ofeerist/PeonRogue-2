using UnityEngine;
using Game.Unit;

namespace Game.Level.UnitData
{
    [CreateAssetMenu(fileName = "New SkeletonData", menuName = "UnitData/Skeleton Data", order = 51)]
    class SkeletonData : ScriptableObject
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

        public void SetData(Unit.Unit unit)
        {
            var health = unit.UnitHealth as EnemyHealth;
            var aiMovement = unit.UnitMovement as AIMovement;
            var aiAttack = unit.UnitAttack as AIAttack;

            health.SetData(_maxHealth.GetValue(), _stanTime.GetValue());
            aiMovement.SetData(_detectionRange.GetValue(), _rotateSpeed.GetValue(), _speed.GetValue());
            aiAttack.SetData(_attackSpeed.GetValue(), _range.GetValue(), _angle.GetValue(), _damage.GetValue());
        }
    }
}

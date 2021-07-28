using UnityEngine;
using Game.Unit;

namespace Game.Level.UnitData
{
    [CreateAssetMenu(fileName = "New NecromancerData", menuName = "UnitData/Necromancer Data", order = 51)]
    class NecromancerData : UnitData
    {
        [SerializeField] private RandomFloat _maxHealth;
        [SerializeField] private RandomFloat _stanTime;

        [Space]

        [SerializeField] private RandomFloat _retreatDistance;
        [SerializeField] private bool _attackOnClamp;

        [SerializeField] private bool _chase;
        [SerializeField] private RandomFloat _minDistanceToChase;
        [SerializeField] private RandomFloat _maxDistanceToChase;

        [SerializeField] private RandomFloat _rotateSpeed;
        [SerializeField] private RandomFloat _speed;

        [Space]

        [SerializeField] private RandomInt _throws;
        [SerializeField] private RandomFloat _angle;
        [SerializeField] private bool _clamped;

        [SerializeField] private RandomFloat _detectDistance;

        [SerializeField] private RandomFloat _minThrowDelay;
        [SerializeField] private RandomFloat _maxThrowDelay;

        [SerializeField] private RandomFloat _damage;
        [SerializeField] private RandomFloat _knockback;
        [SerializeField] private RandomFloat _throwSpeed;
        [SerializeField] private RandomFloat _maxFlightDistance;

        public override void SetData(Unit.Unit unit)
        {
            var health = unit.GetComponent<EnemyHealth>();
            var aiMovement = unit.GetComponent<NecromancerMovement>();
            var aiAttack = unit.GetComponent<NecromancerThrow>();

            health.SetData(_maxHealth.GetValue(), _stanTime.GetValue());
            aiMovement.SetData(_retreatDistance.GetValue(), _chase, _minDistanceToChase.GetValue(), _maxDistanceToChase.GetValue(), _attackOnClamp, _rotateSpeed.GetValue(), _speed.GetValue());
            aiAttack.SetData(_throws.GetValue(), _angle.GetValue(), _clamped, _detectDistance.GetValue(), _minThrowDelay.GetValue(), _maxThrowDelay.GetValue(), _damage.GetValue(), _knockback.GetValue(), _throwSpeed.GetValue(), _maxFlightDistance.GetValue());
        }
    }
}

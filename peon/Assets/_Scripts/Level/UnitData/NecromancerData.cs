using _Scripts.Unit.AI;
using _Scripts.Unit.AI.Necromancer;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

namespace _Scripts.Level.UnitData
{
    [CreateAssetMenu(fileName = "New NecromancerData", menuName = "UnitData/Necromancer Data", order = 51)]
    class NecromancerData : UnitData
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
            var health = unit.GetComponent<AIHealth>();
            var aiMovement = unit.GetComponent<MovementAI>();
            var aiAttack = unit.GetComponent<NecromancerThrow>();

            health.SetData(_maxHealth.GetValue());
            aiMovement.SetData(_speed.GetValue(), _chase, _maxDistanceToChase.GetValue(), _minDistanceToChase.GetValue(), _retreat, _retreatDistance.GetValue());
            aiAttack.SetData(_throws.GetValue(), _angle.GetValue(), _clamped, _detectDistance.GetValue(), _minThrowDelay.GetValue(), _maxThrowDelay.GetValue(), _damage.GetValue(), _knockback.GetValue(), _throwSpeed.GetValue(), _maxFlightDistance.GetValue());
        }
    }
}

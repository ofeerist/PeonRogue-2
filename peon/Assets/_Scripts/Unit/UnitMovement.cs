using UnityEngine;

namespace _Scripts.Unit
{
    public class UnitMovement : MonoCached.MonoCached
    {
        [HideInInspector] public Unit Unit;
        public bool BlockMovement;
        public bool Blocking = false;

        [SerializeField] protected float _rotateSpeed;
        public float Speed;

        public virtual void AddImpulse(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            Unit.Rigidbody.AddForce(direction, ForceMode.Impulse);
            if(stan) Unit.UnitHealth.Stan(stanTime);
        }

        public virtual void AddForce(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            Unit.Rigidbody.AddForce(direction, ForceMode.Force);
            if (stan) Unit.UnitHealth.Stan(stanTime);
        }
    }
}

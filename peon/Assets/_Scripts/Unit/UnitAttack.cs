using UnityEngine;

namespace _Scripts.Unit
{
    public class UnitAttack : MonoCached.MonoCached
    {
        [HideInInspector] public Unit Unit;
        [SerializeField] protected float _Speed;
        public bool InAttack;
    }
}
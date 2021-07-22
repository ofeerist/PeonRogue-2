using UnityEngine;

using Photon.Pun;

namespace Game.Unit
{
    public class UnitAttack : MonoCached
    {
        [HideInInspector] public Unit Unit;
        [SerializeField] protected float _Speed;
        public bool InAttack;
    }
}
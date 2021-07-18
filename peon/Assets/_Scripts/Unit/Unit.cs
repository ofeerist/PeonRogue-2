using UnityEngine;
using Photon.Pun;

namespace Game.Unit
{
    public class Unit : MonoBehaviour
    {
        [HideInInspector] public UnitHealth UnitHealth;
        [HideInInspector] public UnitMovement UnitMovement;
        [HideInInspector] public UnitAttack UnitAttack;

        [HideInInspector] public Animator Animator;
        [HideInInspector] public CharacterController Controller;
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public PhotonView PhotonView;

        private bool _isHooked;
        public bool IsHooked { get { return _isHooked; } set { _isHooked = value; } }
        private void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            Controller = GetComponent<CharacterController>();
            Rigidbody = GetComponent<Rigidbody>();
            PhotonView = GetComponent<PhotonView>();

            UnitHealth = GetComponent<UnitHealth>();
            if (UnitHealth != null) UnitHealth.Unit = this;

            UnitMovement = GetComponent<UnitMovement>();
            if (UnitMovement != null) UnitMovement.Unit = this;

            UnitAttack = GetComponent<UnitAttack>();
            if (UnitAttack != null) UnitAttack.Unit = this;
        }
    }
}
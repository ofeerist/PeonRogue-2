using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Game.Level._Interactable._Talents.Data;

namespace Game.Unit
{
    public class Unit : MonoCached
    {
        [HideInInspector] public UnitHealth UnitHealth;
        [HideInInspector] public UnitMovement UnitMovement;
        [HideInInspector] public UnitAttack UnitAttack;

        [HideInInspector] public Animator Animator;
        [HideInInspector] public CharacterController Controller;
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public PhotonView PhotonView;

        public Dictionary<uint, Skill> Skills = new Dictionary<uint, Skill>();

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

        [PunRPC]
        public void ChangeColor(int peonViewId, float r, float g, float b, float a)
        {
            var peon = PhotonNetwork.GetPhotonView(peonViewId).gameObject;
            var skinnedMesh = peon.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMesh.material.SetColor("TeamColor", new Color(r, g, b, a));

            if (peon.GetPhotonView().Owner != PhotonNetwork.LocalPlayer) peon.GetComponentInChildren<Light>().gameObject.SetActive(false);
        }
    }
}
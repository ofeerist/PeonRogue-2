using System.Collections.Generic;
using _Scripts.Level.Interactable.Talents.Data;
using _Scripts.Unit.Player;
using KinematicCharacterController;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Unit
{
    public class Unit : MonoCached.MonoCached
    {
        [HideInInspector] public UnitHealth UnitHealth;
        [HideInInspector] public UnitMovement UnitMovement;
        [HideInInspector] public UnitAttack UnitAttack;

        [HideInInspector] public Animator Animator;
        [HideInInspector] public KinematicCharacterMotor Controller;
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public PhotonView PhotonView;

        public Camera Camera;
        public UnitState CurrentState;
        
        public Dictionary<uint, Skill> Skills = new Dictionary<uint, Skill>();

        private bool _isHooked;
        public bool IsHooked { get { return _isHooked; } set { _isHooked = value; } }
        private void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            Controller = GetComponent<KinematicCharacterMotor>();
            Rigidbody = GetComponent<Rigidbody>();
            PhotonView = GetComponent<PhotonView>();
        }

        [PunRPC]
        public void ChangeColor(int peonViewId, float r, float g, float b, float a)
        {
            var peon = PhotonNetwork.GetPhotonView(peonViewId).gameObject;
            var skinnedMesh = peon.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMesh.material.SetColor("TeamColor", new UnityEngine.Color(r, g, b, a));

            if (peon.GetPhotonView().Owner != PhotonNetwork.LocalPlayer) peon.GetComponentInChildren<Light>().gameObject.SetActive(false);
        }

        [PunRPC]
        public void DontDestroy()
        {
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(Camera.transform.parent.gameObject);
        }
    }
}
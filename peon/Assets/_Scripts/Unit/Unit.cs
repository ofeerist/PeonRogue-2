using System.Collections.Generic;
using _Scripts.Level.Interactable.Talents.Data;
using KinematicCharacterController;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Unit
{
    public class Unit : MonoCached.MonoCached
    {
        [HideInInspector] public Animator Animator;
        [HideInInspector] public KinematicCharacterMotor Controller;
        [HideInInspector] public Rigidbody Rigidbody;
        [HideInInspector] public PhotonView PhotonView;

        public Camera Camera;
        public UnitState CurrentState;

        public float TimeToStan;
        public float BounceDamage;
        
        public Dictionary<uint, Skill> Skills = new Dictionary<uint, Skill>();
        
        public bool CanBeHooked;
        private static readonly int TeamColor = Shader.PropertyToID("TeamColor");

        private void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            Controller = GetComponent<KinematicCharacterMotor>();
            Rigidbody = GetComponent<Rigidbody>();
            PhotonView = GetComponent<PhotonView>();
        }

        public void SetPosition(Vector3 position)
        {
            Controller.SetPosition(position);
        }
        
        [PunRPC]
        public void ChangeColor(int peonViewId, float r, float g, float b, float a)
        {
            var peon = PhotonNetwork.GetPhotonView(peonViewId).gameObject;
            var skinnedMesh = peon.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMesh.material.SetColor(TeamColor, new UnityEngine.Color(r, g, b, a));

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
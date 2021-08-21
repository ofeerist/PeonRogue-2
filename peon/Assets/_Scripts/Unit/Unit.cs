using System.Collections.Generic;
using _Scripts.Level.Interactable.Talents.Data;
using KinematicCharacterController;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Unit
{
    public class Unit : MonoBehaviour
    {
        [HideInInspector] public Animator Animator;
        [HideInInspector] public KinematicCharacterMotor Controller;
        [HideInInspector] public PhotonView PhotonView;

        public Camera Camera;
        public UnitState CurrentState;

        public float TimeToStan;
        public float BounceDamage;
        
        public Dictionary<uint, Skill> Skills;
        
        public bool CanBeHooked;
        private static readonly int TeamColor = Shader.PropertyToID("TeamColor");

        private int _gold;
        private int Gold
        {
            get => _gold;

            set
            {
                _gold = value;
                OnGoldChanged?.Invoke(value);
            }
        }

        public delegate void GoldChanged(int gold);
        public event GoldChanged OnGoldChanged;
        
        private void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            Controller = GetComponent<KinematicCharacterMotor>();
            PhotonView = GetComponent<PhotonView>();
        }

        public void ReceiveGold(int count)
        {
            Gold += count;
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
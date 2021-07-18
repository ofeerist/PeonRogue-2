using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class LobbyShow : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject LobbyPanel;
        public override void OnJoinedRoom()
        {
            LobbyPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
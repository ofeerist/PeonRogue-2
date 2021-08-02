using Photon.Pun;
using UnityEngine;

namespace _Scripts.UI
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
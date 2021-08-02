using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    class LobbyLeave : MonoBehaviour
    {
        [SerializeField] private Button _leaveButton;

        private void Start()
        {
            _leaveButton.onClick.AddListener(() =>
            {
                PhotonNetwork.LeaveRoom();
            });
        }
    }
}

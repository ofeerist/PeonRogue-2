using UnityEngine;
using Photon.Pun;
using TMPro;

namespace Game.UI
{
    class PingCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMesh;
        private void Update()
        {
            if(PhotonNetwork.IsConnectedAndReady)
                _textMesh.text = PhotonNetwork.GetPing() + " ms";
        }
    }
}

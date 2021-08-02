using Photon.Pun;
using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    class PingCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMesh;
        private void Start()
        {
            InvokeRepeating(nameof(UpdatePingLabel), 0, 1f);
        }

        private void UpdatePingLabel()
        {
            if (PhotonNetwork.IsConnectedAndReady)
                _textMesh.text = PhotonNetwork.GetPing() + " ms";
        }
    }
}

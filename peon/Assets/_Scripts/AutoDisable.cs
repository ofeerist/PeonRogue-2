using Photon.Pun;
using UnityEngine;

namespace _Scripts
{
    public class AutoDisable : MonoBehaviour
    {
        private PhotonView _photonView;

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
        }

        private void Update()
        {
            if (!_photonView.IsMine) gameObject.SetActive(false);
        }
    }
}

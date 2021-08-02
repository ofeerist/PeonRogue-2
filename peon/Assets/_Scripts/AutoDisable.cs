using Photon.Pun;
using UnityEngine;

namespace _Scripts
{
    public class AutoDisable : MonoBehaviour
    {
        private PhotonView _photonView;

        void Start()
        {
            _photonView = GetComponent<PhotonView>();
        }
        void Update()
        {
            if (!_photonView.IsMine) gameObject.SetActive(false);
        }
    }
}

using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts
{
    public class AutoDisable : MonoBehaviour
    {
        private PhotonView _photonView;

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();

            if (!_photonView.IsMine) gameObject.SetActive(false);
            
            /*
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (!_photonView.IsMine) gameObject.SetActive(false);
            }).AddTo(this);
            */
        }
    }
}

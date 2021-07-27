
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Level
{
    class TransferPosition : MonoCached, IInteractable
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _distance;
        [SerializeField] private Image _image;

        public event IInteractable.Interacted OnInteract;

        private void Transfer()
        {
            
        }

        public void Interact()
        {
            OnInteract?.Invoke();
            Transfer();
        }
    }
}

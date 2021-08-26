using Photon.Pun;
using UnityEngine;

namespace _Scripts.Level.Interactable
{
    public class Interactable : MonoBehaviour, IInteractable
    {
        public delegate void Interacted(Interactable interactable);
        public event Interacted OnInteract;

        public SpawnPosition ArrowPosition;

        public PhotonView PhotonView;
        
        public void Interact()
        {
            OnInteract?.Invoke(this);
            OnInteraction();
        }

        protected virtual void OnInteraction()
        {

        }
    }
}

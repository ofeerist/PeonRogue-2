using Photon.Pun;

namespace _Scripts.Level.Interactable
{
    public class Interactable : MonoCached.MonoCached, IInteractable
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

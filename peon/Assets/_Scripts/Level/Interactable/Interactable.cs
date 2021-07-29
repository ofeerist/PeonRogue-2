namespace Game.Level._Interactable
{
    class Interactable : MonoCached, IInteractable
    {
        public delegate void Interacted(Interactable interactable);
        public event Interacted OnInteract;
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

namespace Game.Level
{
    internal interface IInteractable
    {
        public delegate void Interacted();
        public event Interacted OnInteract;
        public void Interact();
    }
}
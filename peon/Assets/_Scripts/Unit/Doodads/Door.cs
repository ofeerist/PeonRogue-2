using UnityEngine;

namespace _Scripts.Unit.Doodads
{
    public class Door : MonoCached.MonoCached
    {
        private Animator _animator;

        private bool IsOpen = false;
        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _animator.SetBool("IsOpen", IsOpen);
        }

        public void Open()
        {
            if(!IsOpen) _animator.SetTrigger("Open");
            IsOpen = true;
            _animator.SetBool("IsOpen", IsOpen);
        }

        public void Close()
        {
            if(IsOpen) _animator.SetTrigger("Close");
            IsOpen = false;
            _animator.SetBool("IsOpen", IsOpen);
        }
    }
}
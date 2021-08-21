using UnityEngine;

namespace _Scripts.Unit.Doodads
{
    public class Door : MonoBehaviour
    {
        private Animator _animator;

        private bool IsOpen = false;
        private static readonly int Open1 = Animator.StringToHash("Open");
        private static readonly int IsOpen1 = Animator.StringToHash("IsOpen");
        private static readonly int Close1 = Animator.StringToHash("Close");

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _animator.SetBool(IsOpen1, IsOpen);
        }

        public void Open()
        {
            if(!IsOpen) _animator.SetTrigger(Open1);
            IsOpen = true;
            _animator.SetBool(IsOpen1, IsOpen);
        }

        public void Close()
        {
            if(IsOpen) _animator.SetTrigger(Close1);
            IsOpen = false;
            _animator.SetBool(IsOpen1, IsOpen);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Unit
{
    public class Door : MonoBehaviour
    {
        private Animator _animator;

        private bool IsOpen;
        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            _animator.SetBool("IsOpen", IsOpen);
        }

        public void Open()
        {
            if(!IsOpen) _animator.SetTrigger("Open");
            IsOpen = true;
        }

        public void Close()
        {
            if(IsOpen) _animator.SetTrigger("Close");
            IsOpen = false;
        }
    }
}
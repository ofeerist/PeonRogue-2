using UnityEngine;

namespace Game.Unit
{
    class AnimatorDisabler : MonoBehaviour
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponentInParent<Animator>();
        }
        private void OnBecameInvisible()
        {
            _animator.enabled = false;
        }

        private void OnBecameVisible()
        {
            _animator.enabled = true;
        }
    }
}

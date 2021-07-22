using UnityEngine;

namespace Game.Unit
{
    class RandomIdle : MonoCached
    {
        private Animator _animator;

        [SerializeField] private int min;
        [SerializeField] private int max;
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        protected override void OnTick()
        {
            _animator.SetInteger("RandomIdle", Random.Range(min, max));
        }
    }
}

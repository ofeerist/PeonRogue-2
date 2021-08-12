using UnityEngine;

namespace _Scripts.Unit.Player
{
    class RandomIdle : MonoCached.MonoCached
    {
        private Animator _animator;

        [SerializeField] private int min;
        [SerializeField] private int max;
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdateRandom()
        {
            _animator.SetInteger("RandomIdle", Random.Range(min, max));
        }
    }
}

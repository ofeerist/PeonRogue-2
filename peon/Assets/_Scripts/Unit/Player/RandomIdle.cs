using UnityEngine;

namespace Game.Unit
{
    class RandomIdle : MonoBehaviour
    {
        private Animator _animator;

        [SerializeField] private int min;
        [SerializeField] private int max;
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            _animator.SetInteger("RandomIdle", Random.Range(min, max));
        }
    }
}

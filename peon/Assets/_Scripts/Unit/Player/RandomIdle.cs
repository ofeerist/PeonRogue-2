using UnityEngine;

namespace _Scripts.Unit.Player
{
    public class RandomIdle : MonoBehaviour
    {
        private Animator _animator;

        [SerializeField] private int min;
        [SerializeField] private int max;
        private static readonly int Idle = Animator.StringToHash("RandomIdle");

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdateRandom()
        {
            _animator.SetInteger(Idle, Random.Range(min, max));
        }
    }
}

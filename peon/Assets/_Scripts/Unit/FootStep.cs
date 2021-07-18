using UnityEngine;

namespace Game.Unit
{
    class FootStep : MonoBehaviour
    {
        private AudioSource _audioSource;

        [SerializeField] private AudioClip[] _clips;
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Step()
        {
            _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
        }
    }
}

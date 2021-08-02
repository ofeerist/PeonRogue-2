using UnityEngine;

namespace _Scripts.Unit
{
    class FootStep : MonoBehaviour
    {
        private AudioSource _audioSource;

        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private AudioClip[] _additional;
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Step()
        {
            _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);

            if(_additional.Length == _clips.Length) _audioSource.PlayOneShot(_additional[Random.Range(0, _additional.Length)]);
        }
    }
}

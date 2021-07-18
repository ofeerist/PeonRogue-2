using UnityEngine;
using UnityEngine.Audio;

namespace Game.UI
{
    class VolumePrefs : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup[] _groups;
        public AudioMixerGroup[] Groups { get { return _groups; } }

        private void Start()
        {
            for (int i = 0; i < _groups.Length; i++)
            {
                var name = _groups[i].name;
                _mixer.SetFloat(name, PlayerPrefs.GetFloat(PrefsConstants.Volumes[i]));
            }
        }
    }
}

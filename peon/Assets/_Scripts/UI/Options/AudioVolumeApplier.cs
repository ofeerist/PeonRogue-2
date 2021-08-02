using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace _Scripts.UI.Options
{
    class AudioVolumeApplier : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private VolumePrefs _volumePrefs;
        [SerializeField] private Slider[] _volumeSliders;

        private void Start()
        {
            for (int i = 0; i < _volumePrefs.Groups.Length; i++)
            {
                var parameterName = _volumePrefs.Groups[i].name;
                var volumePrefName = PrefsConstants.Volumes[i];

                _volumeSliders[i].value = PlayerPrefs.GetFloat(PrefsConstants.Volumes[i]);

                _volumeSliders[i].onValueChanged.AddListener((value) => {
                    _mixer.SetFloat(parameterName, value);
                    PlayerPrefs.SetFloat(volumePrefName, value);
                });
            }
        }
    }
}

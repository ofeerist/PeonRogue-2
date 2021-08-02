using UnityEngine;

namespace _Scripts.UI.Options
{
    class QualityPrefs : MonoBehaviour
    {
        private void Start()
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(PrefsConstants.QualityLevel)); 
        }
    }
}

using UnityEngine;

namespace Game.UI
{
    class QualityPrefs : MonoBehaviour
    {
        private void Start()
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(PrefsConstants.QualityLevel)); 
        }
    }
}

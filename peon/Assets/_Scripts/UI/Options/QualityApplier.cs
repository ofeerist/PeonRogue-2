using TMPro;
using UnityEngine;

namespace _Scripts.UI.Options
{
    class QualityApplier : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown QualityLevel;

        private void Start()
        {
            Application.targetFrameRate = -1;

            QualityLevel.onValueChanged.AddListener((i) => {
                QualitySettings.SetQualityLevel(i);
                PlayerPrefs.SetInt(PrefsConstants.QualityLevel, i);
            });

            QualityLevel.value = QualitySettings.GetQualityLevel();
        }
    }
}

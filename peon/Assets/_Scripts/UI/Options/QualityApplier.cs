using TMPro;
using UnityEngine;

namespace Game.UI
{
    class QualityApplier : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown QualityLevel;

        private void Start()
        {
            Application.targetFrameRate = 150;

            QualityLevel.onValueChanged.AddListener((i) => {
                QualitySettings.SetQualityLevel(i);
                PlayerPrefs.SetInt(PrefsConstants.QualityLevel, i);
            });

            QualityLevel.value = QualitySettings.GetQualityLevel();
        }
    }
}

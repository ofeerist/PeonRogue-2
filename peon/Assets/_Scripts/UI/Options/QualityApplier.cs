using TMPro;
using UnityEngine;

namespace Game.UI
{
    class QualityApplier : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown QualityLevel;

        private void Start()
        {
            QualityLevel.onValueChanged.AddListener((i) => {
                QualitySettings.SetQualityLevel(i);
                PlayerPrefs.SetInt(PrefsConstants.QualityLevel, i);
            });

            QualityLevel.value = QualitySettings.GetQualityLevel();
        }
    }
}

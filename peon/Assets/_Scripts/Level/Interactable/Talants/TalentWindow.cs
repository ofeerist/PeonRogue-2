
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Level._Interactable._Talants
{
    [System.Serializable]
    class TalentView
    {
        public Image Image;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Description;
    }
    class TalentWindow : MonoBehaviour
    {
        [SerializeField] private GameObject _window;

        [Space]

        [SerializeField] private TalentView[] _talantViews;
        [SerializeField] private Image _selection;
        [SerializeField] private TextMeshProUGUI _toolTip;

        public void Add(Talent talant)
        {
            if (_window.activeSelf) throw new UnityException("Multiply add is not allowed!");
            _window.SetActive(true);
        }
    }
}

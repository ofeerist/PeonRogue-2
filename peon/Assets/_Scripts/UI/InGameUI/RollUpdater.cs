using System.Collections;
using _Scripts.Unit.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class RollUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private Image _regenerateImage;
        private void Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            _unitObserver.UnitChanged += UnitObserverOnUnitChanged;
        }

        private void UnitObserverOnUnitChanged(Unit.Unit u)
        {
            var obj = u.GetComponent<RollAttack>();
            
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.RollingTime);
            
            obj.TimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(float time)
        {
            _regenerateImage.fillAmount = 1 - time;
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();
        }
    }
}

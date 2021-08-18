using _Scripts.Unit.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class ThrowUpdater : MonoBehaviour
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
            var obj = u.GetComponent<AxeThrow>();
            
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.CurrentThrowCharges);
            
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



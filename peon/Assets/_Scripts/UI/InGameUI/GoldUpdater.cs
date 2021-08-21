using System.Collections;
using _Scripts.Unit.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    class GoldUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private void Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            _unitObserver.UnitChanged += UnitObserverOnUnitChanged;
        }

        private void UnitObserverOnUnitChanged(Unit.Unit u)
        {
            u.OnGoldChanged += UpdateCharge;
            UpdateCharge(u.Gold);
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();
        }
    }
}

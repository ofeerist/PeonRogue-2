using UnityEngine;
using TMPro;
using Game.Unit;
using System.Collections;

namespace Game.UI.InGameUI
{
    class SlamUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;
        private IEnumerator Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            yield return new WaitUntil(() => _unitObserver.Unit != null);
            var obj = _unitObserver.Unit.GetComponent<Slam>();
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.CurrentCharges);
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();
        }
    }
}

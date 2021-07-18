using UnityEngine;
using TMPro;
using Game.Unit;
using System.Collections;

namespace Game.UI.InGameUI
{
    class RollUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;
        private IEnumerator Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            yield return new WaitUntil(() => _unitObserver.Unit != null);
            var obj = _unitObserver.Unit.GetComponent<PlayerAxeAttack>();
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.RollingTime);
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();
        }
    }
}

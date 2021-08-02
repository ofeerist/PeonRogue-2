using System.Collections;
using _Scripts.Unit.Player;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.InGameUI
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

﻿using UnityEngine;
using TMPro;
using Game.Unit;
using System.Collections;

namespace Game.UI.InGameUI
{
    class DashUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;
        private IEnumerator Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            yield return new WaitUntil(() => _unitObserver.Unit != null);
            var obj = _unitObserver.Unit.GetComponent<PlayerMovement>();
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.DashCurrentStock);
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();
        }
    }
}

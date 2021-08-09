using System;
using _Scripts.UI.InGameUI;
using _Scripts.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private UnitObserver _unitObserver;

        private bool _changed;

        private float _currentValue;

        [SerializeField] private float _speed;
        
        private void Update()
        {
            _image.fillAmount = Mathf.Lerp(_image.fillAmount, _currentValue, _speed * Time.deltaTime);
            
            if (_changed) return;

            if (_unitObserver.Unit != null)
            {
                _unitObserver.Unit.UnitHealth.OnValueChanged += ValueChanged;
                _changed = true;
            }
        }

        private void ValueChanged(float value)
        {
            _currentValue = value / _unitObserver.Unit.UnitHealth._maxHealth;
        }
    }
}
using System;
using System.Globalization;
using _Scripts.UI.InGameUI;
using _Scripts.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _maxHP;
        [SerializeField] private TextMeshProUGUI _currentHP;
        [SerializeField] private TextMeshProUGUI _regen;
        
        private bool _changed;

        private float _currentValue;

        [SerializeField] private float _speed;
        
        private void Update()
        {
            _image.fillAmount = Mathf.Lerp(_image.fillAmount, _currentValue, _speed * Time.deltaTime);
            
            if (_changed) return;

            if (_unitObserver.Unit != null)
            {
                _unitObserver.Unit.UnitHealth.MaxHealthChanged += MaxHealthChanged;
                _unitObserver.Unit.UnitHealth.HealthChanged += HealthChanged;
                _unitObserver.Unit.UnitHealth.RegenChanged += RegenChanged;

                _maxHP.text = _unitObserver.Unit.UnitHealth.MaxHealth.ToString(CultureInfo.InvariantCulture);
                _currentHP.text = _unitObserver.Unit.UnitHealth.CurrentHealth.ToString(CultureInfo.InvariantCulture);
                _regen.text = _unitObserver.Unit.UnitHealth.Regen.ToString(CultureInfo.InvariantCulture);
                
                _changed = true;
            }
        }

        private void MaxHealthChanged(float value)
        {
            _maxHP.text = Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
        }

        private void RegenChanged(float value)
        {
            _regen.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void HealthChanged(float value)
        {
            _currentValue = value / _unitObserver.Unit.UnitHealth.MaxHealth;
            _currentHP.text = Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
        }
    }
}
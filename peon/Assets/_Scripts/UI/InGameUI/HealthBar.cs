using System.Globalization;
using _Scripts.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _maxHP;
        [SerializeField] private TextMeshProUGUI _currentHP;
        [SerializeField] private TextMeshProUGUI _regen;
        
        private float _currentValue;

        [SerializeField] private float _speed;

        [SerializeField] private Graphic[] _toColorize;
        [SerializeField] private Gradient _gradient;
        
        private void Start()
        {
            _unitObserver.UnitChanged += OnUnitChanged;
        }

        private void OnUnitChanged(Unit.Unit u)
        {
            var h = u.GetComponent<UnitHealth>();
            
            h.MaxHealthChanged += MaxHealthChanged;
            h.HealthChanged += HealthChanged;
            h.RegenChanged += RegenChanged;

            _maxHP.text = h.MaxHealth.ToString(CultureInfo.InvariantCulture);
            _currentHP.text = h.CurrentHealth.ToString(CultureInfo.InvariantCulture);
            _regen.text = h.Regen.ToString(CultureInfo.InvariantCulture);
        }

        private void Update()
        {
            _image.fillAmount = Mathf.Lerp(_image.fillAmount, _currentValue, _speed * Time.deltaTime);
            foreach (var image in _toColorize)
            {
                image.color = _gradient.Evaluate(_image.fillAmount);
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
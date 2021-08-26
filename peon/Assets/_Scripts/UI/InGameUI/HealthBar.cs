using System.Globalization;
using _Scripts.Unit;
using _Scripts.Unit.Player;
using TMPro;
using UniRx;
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

        private PlayerHealth _playerHealth;
        
        private void Start()
        {
            _unitObserver.UnitChanged += OnUnitChanged;

            Observable.EveryUpdate().Subscribe(x =>
            {
                _image.fillAmount = Mathf.Lerp(_image.fillAmount, _currentValue, _speed * Time.deltaTime);
                foreach (var image in _toColorize)
                {
                    image.color = _gradient.Evaluate(_image.fillAmount);
                }
            }).AddTo(this);
        }

        private void OnUnitChanged(Unit.Unit u)
        {
            _playerHealth = u.GetComponent<PlayerHealth>();
            
            _playerHealth.MaxHealthChanged += MaxHealthChanged;
            _playerHealth.HealthChanged += HealthChanged;
            _playerHealth.RegenChanged += RegenChanged;

            _maxHP.text = _playerHealth.MaxHealth.ToString(CultureInfo.InvariantCulture);
            _currentHP.text = _playerHealth.CurrentHealth.ToString(CultureInfo.InvariantCulture);
            _regen.text = _playerHealth.Regen.ToString(CultureInfo.InvariantCulture);
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
            _currentValue = value / _playerHealth.MaxHealth;
            _currentHP.text = Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
        }
    }
}
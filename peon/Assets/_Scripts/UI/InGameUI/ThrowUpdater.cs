using _Scripts.Unit.Player;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class ThrowUpdater : MonoBehaviour
    {
        private UnitObserver _unitObserver;

        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private Image _regenerateImage;
        
        [SerializeField] private Image _border;
        [SerializeField] private float _transmutationSpeed;
        
        private UnityEngine.Color _currentColor;
        private int currentCharges;

        private void Start()
        {
            _unitObserver = GetComponentInParent<UnitObserver>();

            _unitObserver.UnitChanged += UnitObserverOnUnitChanged;

            _currentColor = _border.color;
        }
        
        private void UnitObserverOnUnitChanged(Unit.Unit u)
        {
            var obj = u.GetComponent<AxeThrow>();
            
            obj.ChargeChanged += UpdateCharge;
            UpdateCharge(obj.CurrentThrowCharges);
            
            obj.TimeChanged += OnTimeChanged;
            
            obj.Overtapping += OnOvertapping;

            Observable.EveryUpdate().Subscribe(x =>
            {
                _border.color =
                    UnityEngine.Color.Lerp(_border.color, _currentColor, _transmutationSpeed * Time.deltaTime);
            }).AddTo(this);
        }

        private void OnOvertapping()
        {
            _border.color = UnityEngine.Color.red;
        }

        private void OnTimeChanged(float time)
        {
            _regenerateImage.fillAmount = 1 - time;
            
            _currentColor.a = time;
        }

        private void UpdateCharge(int charges)
        {
            _textMesh.text = charges.ToString();

            if (currentCharges > charges)
                _border.color = UnityEngine.Color.yellow * new UnityEngine.Color(1, 1, 1, 0) +
                                new UnityEngine.Color(0, 0, 0, _currentColor.a);
            if (currentCharges < charges) _border.color = UnityEngine.Color.green* new UnityEngine.Color(1, 1, 1, 0) +
                                                          new UnityEngine.Color(0, 0, 0, _currentColor.a);;
            
            currentCharges = charges;
        }
    }
}



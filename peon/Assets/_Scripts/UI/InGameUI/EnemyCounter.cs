using System;
using _Scripts.Level;
using _Scripts.Unit.AI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class EnemyCounter : MonoBehaviour
    {
        [SerializeField] private LevelFaсtory _levelFactory;

        [SerializeField] private TextMeshProUGUI _current;
        [SerializeField] private TextMeshProUGUI _max;
        [SerializeField] private GameObject _parent; 
        
        [Space] 
        [SerializeField] private Image _slider;
        [SerializeField] private Image _forwardSlider;
        
        [SerializeField] private LayerMask _enemy;
        [SerializeField] private float _distance;
        private readonly Collider[] _results = new Collider[5];

        private Transform _target;

        [SerializeField] private float _changeAmountSpeed;
        [SerializeField] private float _changeRotationSpeed;

        private void Start()
        {
            _levelFactory.EnemyCountChanged += EnemyCount;
            _levelFactory.WaveStarted += OnWaveStarted;
            _levelFactory.WaveEnded += OnWaveEnded;

            var sliderRect = _slider.rectTransform;
            var sliderParent = sliderRect.parent;

            var forwardSlider = _forwardSlider.rectTransform.parent;
            
            _levelFactory.UnitObserver.UnitChanged += delegate(Unit.Unit unit)
            {
                var _transform = unit.transform;
                
                Observable.EveryUpdate().Subscribe(x =>
                {
                    forwardSlider.localRotation = Quaternion.Euler(0, 180, Quaternion.LookRotation(_transform.forward, Vector3.up).eulerAngles.y);
                    
                    if (_target == null)
                    {
                        _slider.fillAmount = 0f;
                        return;
                    }
                    
                    var position = _transform.position;
                    var target = _target.position;

                    var amount = .3f - Vector3.Distance(position, target) / _distance;
                    amount = Mathf.Clamp(amount, .015f, .3f);
                    
                    _slider.fillAmount = Mathf.Lerp(_slider.fillAmount, amount, _changeAmountSpeed * Time.deltaTime);
                    sliderRect.localRotation = Quaternion.Euler(0, 0, amount * 180);

                    var dir = (target - position).normalized;
                    var rotation = Quaternion.LookRotation(dir, Vector3.up);

                    var localRotation = sliderParent.rotation;
                    localRotation = Quaternion.Euler(localRotation.eulerAngles.x, localRotation.eulerAngles.y,
                        Mathf.Lerp(localRotation.eulerAngles.z, rotation.eulerAngles.y,
                            _changeRotationSpeed * Time.deltaTime));
                    sliderParent.rotation = localRotation;
                }).AddTo(this);
            
                Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
                {
                    var size = Physics.OverlapSphereNonAlloc(_transform.position, _distance, _results, _enemy);
                    var min = Mathf.Infinity;

                    if (size != 0)
                    {
                        var position = _transform.position;
                        for (int i = 0; i < size; i++)
                        {
                            var distance = Vector3.Distance(position, _results[i].transform.position);
                            if (distance < min)
                            {
                                var health = _results[i].gameObject.GetComponent<AIHealth>();
                                if (health == null || health.IsDoodad) continue;

                                _target = _results[i].transform;
                                min = distance;
                            }
                        }
                    }
                    else _target = null;
                }).AddTo(this);
            };
        }

        private void OnWaveEnded()
        {
            _parent.SetActive(false);
        }

        private void OnWaveStarted(int enemys)
        {
            _max.text = enemys.ToString();
            
            _parent.SetActive(true);
            EnemyCount(enemys);
        }

        private void EnemyCount(int count)
        {
            _current.text = count.ToString();
        }
    }
}
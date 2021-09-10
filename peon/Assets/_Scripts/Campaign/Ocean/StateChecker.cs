using System;
using _Scripts.UI;
using UniRx;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.Campaign.Ocean
{
    public class StateChecker : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private DarknessGroupTransition _diffcultDarkness;
        [SerializeField] private DarknessGroupTransition _titleDarkness;
        [SerializeField] private ShipMovement _shipMovement;

        [Space] 
        
        [SerializeField] private Button _easy;
        [SerializeField] private GameObject _easyDesc;
        [SerializeField] private Button _medium;
        [SerializeField] private GameObject _mediumDesc;
        [SerializeField] private Button _hard;
        [SerializeField] private GameObject _hardDesc;

        private void AllDisable()
        {
            _diffcultDarkness.DeactivateDark();
            _easy.interactable = _medium.interactable = _hard.interactable = false;
            
            Observable.Timer(TimeSpan.FromSeconds(4f)).Subscribe(x =>
            {
                _titleDarkness.DeactivateDark();
                _shipMovement.enabled = true;
                _musicSource.Play();
            }).AddTo(this);
        }
        
        private void Start()
        {
            _shipMovement.enabled = false;
            
            _easy.onClick.AddListener(delegate
            {
                _shipMovement.MaxDifference = 0.458f / 2f;
                AllDisable();
            });
            
            _medium.onClick.AddListener(delegate
            {
                _shipMovement.MaxDifference = 0.186f;
                AllDisable();
            });
            
            _hard.onClick.AddListener(delegate
            {
                _shipMovement.MaxDifference = 0.15f;
                AllDisable();
            });

            Observable.Timer(TimeSpan.FromSeconds(_musicSource.clip.length)).Subscribe(x =>
            {
                    
            }).AddTo(this);

            Observable.EveryUpdate().Subscribe(x =>
            {
                //print(EventSystem.current.currentSelectedGameObject.name ?? "blyat");
                if (EventSystem.current.currentSelectedGameObject == _easy.gameObject)
                {
                    _easyDesc.SetActive(true);
                    _mediumDesc.SetActive(false);
                    _hardDesc.SetActive(false);
                }
                if (EventSystem.current.currentSelectedGameObject == _medium.gameObject)
                {
                    _easyDesc.SetActive(false);
                    _mediumDesc.SetActive(true);
                    _hardDesc.SetActive(false);
                }
                if (EventSystem.current.currentSelectedGameObject == _hard.gameObject)
                {
                    _easyDesc.SetActive(false);
                    _mediumDesc.SetActive(false);
                    _hardDesc.SetActive(true);
                }
            }).AddTo(this);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class ButtonAnimation : MonoCached, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Tooltip("optional")]
        [SerializeField] private RectTransform _target;

        [SerializeField] private Vector2 _startPosition;
        [SerializeField] private Vector2 _endPosition;
        [SerializeField] private float _speed;

        [SerializeField] private bool _stayOnSelect;
        [SerializeField] private bool _sound = true;
        [SerializeField] private bool _manualListener;

        private Button _button;
        private bool _highlighted;
        private bool _selected;

        private RectTransform _targetTransform;

        private AudioSource _audio;
        private AudioClip _hover;
        private AudioClip _click;
        void Start()
        {
            _button = GetComponent<Button>();
            _targetTransform = _button != null ? _button.targetGraphic.rectTransform : _target != null ? _target : GetComponent<RectTransform>();

            if (_sound) 
                _audio = GameObject.Find("Canvas").GetComponent<AudioSource>();

            _hover = Resources.Load<AudioClip>("Sound/UI/mouseover1");
            _click = Resources.Load<AudioClip>("Sound/UI/mouseclick1");

            if (_button != null && !_manualListener)
            {
                _button.onClick.AddListener(() =>
                {
                    SoundClick();
                });
            }
        }

        public void SoundClick()
        {
            if (_sound)
            {
                _audio.clip = _click;
                _audio.Play();
            }
        }

        protected override void OnTick()
        {
            if (_button != null && !_button.interactable) return;

            if(_highlighted || _selected || (_stayOnSelect && EventSystem.current.currentSelectedGameObject == _targetTransform.gameObject)) _targetTransform.anchoredPosition = Vector2.Lerp(_targetTransform.anchoredPosition, _endPosition, _speed * Time.deltaTime);
            else _targetTransform.anchoredPosition = Vector2.Lerp(_targetTransform.anchoredPosition, _startPosition, _speed * Time.deltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _highlighted = true;
            if (_sound && !_audio.isPlaying)
            {
                _audio.clip = _hover;
                _audio.Play();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlighted = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_stayOnSelect) _selected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_stayOnSelect) _selected = false;
        }
    }
}
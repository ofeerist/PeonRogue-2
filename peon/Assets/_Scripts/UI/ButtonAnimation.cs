using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ButtonAnimation : MonoCached.MonoCached, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
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
        private GameObject _targetGameObject;

        private AudioSource _audio;
        private AudioClip _hover;
        private AudioClip _click;

        private int state = 0;

        void Start()
        {
            _button = GetComponent<Button>();
            _targetTransform = _button != null ? _button.targetGraphic.rectTransform : _target != null ? _target : GetComponent<RectTransform>();

            if (_sound) 
                _audio = GameObject.Find("MenuCanvas").GetComponent<AudioSource>();

            _hover = Resources.Load<AudioClip>("Sound/UI/mouseover1");
            _click = Resources.Load<AudioClip>("Sound/UI/mouseclick1");

            if (_button != null && !_manualListener)
            {
                _button.onClick.AddListener(SoundClick);
            }

            _targetGameObject = _targetTransform.gameObject;
        }

        private void SoundClick()
        {
            if (_sound)
            {
                _audio.clip = _click;
                _audio.Play();
            }
        }

        protected override void OnTick()
        {
            if (_button != null && !_button.interactable  && !_button.gameObject.activeInHierarchy) return;

            if (state == 0 || _stayOnSelect)
            {
                var transform = _targetTransform;
                if (transform == null) return;

                var pos = transform.anchoredPosition;

                if (_highlighted || _selected || (_stayOnSelect && EventSystem.current.currentSelectedGameObject == _targetGameObject))
                {
                    if (Vector2.Distance(pos, _endPosition) > .01f) transform.anchoredPosition = Vector2.Lerp(pos, _endPosition, _speed * Time.deltaTime);
                    else state = 1;
                }
                else
                {
                    if (Vector2.Distance(pos, _startPosition) > .01f) transform.anchoredPosition = Vector2.Lerp(pos, _startPosition, _speed * Time.deltaTime);
                    else state = -1;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _highlighted = true;
            if (_sound && !_audio.isPlaying)
            {
                _audio.clip = _hover;
                _audio.Play();
            }
            state = 0;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlighted = false;
            state = 0;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_stayOnSelect) _selected = true;
            state = 0;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_stayOnSelect) _selected = false;
            state = 0;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class DarknessTransition : MonoBehaviour
    {
        public float Speed;
        private Image _image;
        private bool _dark;
        private void Start()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (_dark) _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 1), Speed * Time.deltaTime);
            else _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 0), Speed * Time.deltaTime);
        }

        public void ActivateDark()
        {
            _dark = true;
        }

        public void ActivateDarkImmediatly()
        {
            _image.color = new Color(0, 0, 0, 1);
            _dark = true;
        }

        public void DeactivateDark()
        {
            _dark = false;
        }

        public void DeactivateDarkImmediatly()
        {
            _image.color = new Color(0, 0, 0, 0);
            _dark = false;
        }
    }
}
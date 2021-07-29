using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class DarknessTransition : MonoCached
    {
        public float Speed;
        private Image _image;
        private bool _dark;
        private void Start()
        {
            _image = GetComponent<Image>();
        }

        protected override void OnTick()
        {
            if (_dark) { if (_image.color.a <= .99f) _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 1), Speed * Time.deltaTime); }
            else { if (_image.color.a >= .01f) _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 0), Speed * Time.deltaTime); }
        }

        public void ActivateDark()
        {
            _dark = true;
        }

        public void ActivateDarkImmediatly()
        {
            GetComponent<Image>().color = new Color(0, 0, 0, 1);
            _dark = true;
        }

        public void DeactivateDark()
        {
            _dark = false;
        }

        public void DeactivateDarkImmediatly()
        {
            GetComponent<Image>().color = new Color(0, 0, 0, 0);
            _dark = false;
        }
    }
}
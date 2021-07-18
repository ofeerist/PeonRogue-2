using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.TextTag
{
    class TextTag : MonoBehaviour
    {
        private float _destroyTime = 0;

        public float LifeTime
        {
            get
            {
                return _destroyTime - Time.time;
            }

            set
            {
                _destroyTime = value + Time.time;
            }
        }

        public Vector3 Velocity;

        private TextMeshProUGUI textMesh;

        public string Text
        {
            get
            {
                return textMesh.text;
            }

            set
            {
                textMesh.text = value;
            }
        }

        public Color Color
        {
            get
            {
                return textMesh.color;
            }

            set
            {
                textMesh.color = value;
            }
        }

        private void Update()
        {
            if (_destroyTime < Time.time) Color = Color.Lerp(Color, new Color(0, 0, 0, 0), Time.deltaTime * 5);
            if (Color.a <= .2f) Destroy(gameObject);
            
            if(Velocity != null) transform.position += Velocity;
        }

        public static TextTag Create(Vector3 position, string text, Color color, float lifeTime, Vector3 velocity, float fontSize = .35f)
        {
            var textTag = Instantiate(Resources.Load<TextTag>("_Prefabs/TextTagv2"));
            textTag.transform.position = position;

            textTag.textMesh = textTag.GetComponentInChildren<TextMeshProUGUI>();
            textTag.Text = text;
            textTag.Color = color;

            textTag.LifeTime = lifeTime;
            textTag.Velocity = velocity;

            textTag.textMesh.fontSize = fontSize;

            return textTag;
        }
    }
}
using UnityEngine;
using TMPro;

namespace Game.TextTag
{
    class TextTag : MonoCached
    {
        private float _destroyTime = 0;

        private static TextTag _textTag;

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
        private bool _destroy;
        private TextMeshProUGUI textMesh;

        private static void Init()
        {
            _textTag = Resources.Load<TextTag>("_Prefabs/TextTagv2");
        }

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

        protected override void OnTick()
        {
            if (_destroy)
            {
                if (_destroyTime < Time.time) Color = Color.Lerp(Color, new Color(0, 0, 0, 0), Time.deltaTime * 5);
                if (Color.a <= .2f) Destroy(gameObject);
            }

            if(Velocity != null) transform.position += Velocity;
        }

        public static TextTag Create(Vector3 position, string text, Color color, float lifeTime, Vector3 velocity, bool destroy, float fontSize = .35f)
        {
            if (_textTag == null) Init();

            var textTag = Instantiate(_textTag);
            textTag.transform.position = position;

            textTag.textMesh = textTag.GetComponentInChildren<TextMeshProUGUI>();
            textTag.Text = text;
            textTag.Color = color;

            textTag.LifeTime = lifeTime;
            textTag.Velocity = velocity;

            textTag._destroy = destroy;

            textTag.textMesh.fontSize = fontSize;

            return textTag;
        }
    }
}
using TMPro;
using UnityEngine;

namespace _Scripts.TextTag
{
    class TextTag : MonoCached.MonoCached
    {
        private float _destroyTime = 0;

        private static TextTag _textTag;

        public float LifeTime
        {
            get => _destroyTime - Time.time;

            set => _destroyTime = value + Time.time;
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
            get => textMesh.text;

            set => textMesh.text = value;
        }

        public UnityEngine.Color Color
        {
            get => textMesh.color;

            set => textMesh.color = value;
        }

        protected override void OnTick()
        {
            var a = Color.a > .01f;
            if (_destroyTime < Time.time && a) Color = UnityEngine.Color.Lerp(Color, new UnityEngine.Color(0, 0, 0, 0), Time.deltaTime * 5);

            if (!a)
            {
                if (_destroy) Destroy(gameObject);

                transform.position += Velocity;
            }
        }

        public static TextTag Create(Vector3 position, string text, UnityEngine.Color color, float lifeTime, Vector3 velocity, bool destroy, float fontSize = .35f)
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
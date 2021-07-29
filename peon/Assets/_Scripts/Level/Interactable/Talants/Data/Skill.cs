using UnityEngine;

namespace Game.Level._Interactable._Talants.Data
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "Skill", order = 52)]
    class Skill : ScriptableObject
    {
        [SerializeField] private Talents _talant;
        public Talents Talant { get { return _talant; } }

        [SerializeField] private Sprite _icon;
        public Sprite Icon { get { return _icon; } }

        [SerializeField] private string _name;
        public string Name { get { return _name; } }

        [SerializeField] private string _desription;
        public string Desription { get { return _desription; } }

        [SerializeField] private string _toolTip;
        public string ToolTip { get { return _toolTip; } }

        [SerializeField] private float[] _data;
        public float[] Data { get { return _data; } }

        [SerializeField] private float _maxLevel;
        public float MaxLevel { get { return _maxLevel; } }

        private float _currentLevel;
        public float CurrentLevel { get { return _currentLevel; } set { _currentLevel = value; } }
    }
}

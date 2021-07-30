using UnityEngine;

namespace Game.Level._Interactable._Talents.Data
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "Skill", order = 52)]
    public class Skill : ScriptableObject
    {
        [SerializeField] private Talents _talant;
        public Talents Talant { get { return _talant; } }

        [SerializeField] private uint _id;
        public uint ID { get { return _id; } }

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

        [SerializeField] private int _maxLevel;
        public int MaxLevel { get { return _maxLevel; } }

        private int _currentLevel;
        public int CurrentLevel { get { return _currentLevel; } set { _currentLevel = value; } }
    }
}

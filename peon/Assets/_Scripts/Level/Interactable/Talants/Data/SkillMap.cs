using System.Collections.Generic;
using UnityEngine;

namespace Game.Level._Interactable._Talents.Data
{
    [CreateAssetMenu(fileName = "New SkillMap", menuName = "SkillMap", order = 53)]
    class SkillMap : ScriptableObject
    {
        private Dictionary<Talents, Skill[]> _map;
        public Dictionary<Talents, Skill[]> Map
        {
            get
            {
                if (_map == null) InitMap();

                return _map;
            }
            private set
            {
                _map = value;
            }
        }

        private void InitMap()
        {
            Map = new Dictionary<Talents, Skill[]>
            {
                { Talents.Thrall, _thrallSkills }
            };
        }

        [Header(nameof(Talents.Thrall))]
        [SerializeField] private Skill[] _thrallSkills;
    }
}

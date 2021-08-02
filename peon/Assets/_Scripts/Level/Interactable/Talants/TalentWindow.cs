using System.Collections.Generic;
using _Scripts.Level.Interactable.Talants.Data;
using _Scripts.UI.InGameUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Level.Interactable.Talants
{
    [System.Serializable]
    class TalentView
    {
        public Image Image;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Description;
        public string ToolTip;
    }
    class TalentWindow : MonoBehaviour
    {
        [SerializeField] private SkillMap _skillMap;
        [SerializeField] private UnitObserver _unitObserver;

        [Space]

        [SerializeField] private GameObject _window;
        [SerializeField] private Sprite _emptySprite;

        [Space]

        [SerializeField] private TalentView[] _talentViews;
        [SerializeField] private Image _selection;
        [SerializeField] private TextMeshProUGUI _toolTip;

        public void Add(Talents talent)
        {
            if (_window.activeSelf) throw new UnityException("Multiply add is not allowed!");
            _window.SetActive(true);

            var currentSkills = new List<Skill>();

            var unit = _unitObserver.Unit;
            if (unit.PhotonView.IsMine)
            {
                for (int i = 0; i < _talentViews.Length; i++)
                {
                    var skill = GetSkill(talent, currentSkills);
                    currentSkills.Add(skill);

                    ClearView(i);
                    if (skill == null) return;
                    if (unit.Skills.TryGetValue(skill.ID, out var unitSkill) && unitSkill.CurrentLevel == skill.MaxLevel) return;
                    
                    _talentViews[i].Image.sprite = skill.Icon;
                    _talentViews[i].Name.text = skill.Name;
                    _talentViews[i].Description.text = string.Format(skill.Desription, skill.Data[0]);
                    _talentViews[i].ToolTip = skill.ToolTip;

                    if (unitSkill)
                        _talentViews[i].Description.text = string.Format(skill.Desription, skill.Data[unitSkill.CurrentLevel]);
                }
            }
        }

        private void ClearView(int index)
        {
            _talentViews[index].Image.sprite = _emptySprite;
            _talentViews[index].Name.text = "";
            _talentViews[index].Description.text = "";
            _talentViews[index].ToolTip = "";
        }

        private Skill GetSkill(Talents talent, List<Skill> current)
        {
            var list = new List<Skill>(_skillMap.Map[talent]);

            for (int i = 0; i < current.Count; i++)
                list.Remove(current?[i]);
            
            return list.Count != 0 ? list[Random.Range(0, list.Count)] : null;
        }
    }
}

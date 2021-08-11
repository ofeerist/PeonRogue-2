using UnityEditorInternal;
using UnityEngine;

namespace _Scripts.UI.InGameUI
{
    public class UnitObserver : MonoBehaviour
    {
        private Unit.Unit _unit;
        public Unit.Unit Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                UnitChanged?.Invoke(value);
            } 
        }

        public delegate void Changed(Unit.Unit u);
        public event Changed UnitChanged;
        
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}

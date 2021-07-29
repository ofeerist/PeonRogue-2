using UnityEngine;

namespace Game.UI.InGameUI
{
    public class UnitObserver : MonoBehaviour
    {
        public Unit.Unit Unit;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}

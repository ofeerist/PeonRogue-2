using UnityEngine;

namespace _Scripts.UI.InGameUI
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

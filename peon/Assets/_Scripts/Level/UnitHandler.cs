using System.Collections.Generic;
using UnityEngine;

namespace Game.Level
{
    class UnitHandler : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public List<Unit.Unit> Units = new List<Unit.Unit>();
    }
}

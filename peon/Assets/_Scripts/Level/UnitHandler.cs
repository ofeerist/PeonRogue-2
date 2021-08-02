using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Level
{
    public class UnitHandler : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public List<Unit.Unit> Units = new List<Unit.Unit>();
    }
}

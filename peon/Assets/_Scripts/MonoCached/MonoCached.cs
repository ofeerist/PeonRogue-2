using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.MonoCached
{
    public class MonoCached : MonoBehaviour
    {
        public static List<MonoCached> allUpdate = new List<MonoCached>(10001);
        public static List<MonoCached> allFixedUpdate = new List<MonoCached>(10001);

        private void OnEnable() => allUpdate.Add(this);
        private void OnDisable() => allUpdate.Remove(this);
        private void OnDestroy() => allUpdate.Remove(this);

        protected void AddFixedUpdate() => allFixedUpdate.Add(this);
        protected void RemoveFixedUpdate() => allFixedUpdate.Remove(this);

        public void FixedTick() => OnFixedTick();
        protected virtual void OnFixedTick() { }

        public void Tick() => OnTick();
        protected virtual void OnTick() { }
    }
}


using UnityEngine;

namespace _Scripts.MonoCached
{
    class GlobalUpdate : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
        private void Update()
        {
            for (int i = 0; i < MonoCached.allUpdate.Count; i++) MonoCached.allUpdate[i].Tick();

            for (int i = 0; i < MonoCached.allFixedUpdate.Count; i++) MonoCached.allFixedUpdate[i].FixedTick();
        }
    }
}


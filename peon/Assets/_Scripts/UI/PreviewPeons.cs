using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    [System.Serializable]
    class PreviewPeon
    {
        public GameObject GameObject;
        public SkinnedMeshRenderer MeshRenderer;
        public TextMeshProUGUI TextName;
    }
    class PreviewPeons : MonoBehaviour
    {
        [SerializeField] private GameObject[] GameObjects;
        public PreviewPeon[] Peons;

        private void Start()
        {
            Peons = new PreviewPeon[GameObjects.Length];
            for (int i = 0; i < GameObjects.Length; i++)
            {
                Peons[i] = new PreviewPeon
                {
                    GameObject = GameObjects[i],
                    MeshRenderer = GameObjects[i].GetComponentInChildren<SkinnedMeshRenderer>(),
                    TextName = GameObjects[i].GetComponentInChildren<TextMeshProUGUI>()
                };
            }
        }
    }
}

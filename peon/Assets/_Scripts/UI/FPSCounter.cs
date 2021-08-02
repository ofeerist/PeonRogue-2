using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    class FPSCounter : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        private void Start()
        {
            InvokeRepeating(nameof(UpdateFPSLabel), 0, .1f);
        }

        private void UpdateFPSLabel()
        {
            _textMesh.text = (int)(1f / Time.unscaledDeltaTime) + " FPS";
        }
    }
}

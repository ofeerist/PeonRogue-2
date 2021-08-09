using _Scripts.Level;
using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    public class EnemyCounter : MonoBehaviour
    {
        [SerializeField] private LevelFaсtory _levelFactory;

        [SerializeField] private TextMeshProUGUI _textMesh;
        
        private void Start()
        {
            _levelFactory.EnemyCountChanged += EnemyCount;
        }

        private void EnemyCount(int count)
        {
            _textMesh.text = count.ToString();
        }
    }
}
using _Scripts.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class EnemyCounter : MonoBehaviour
    {
        [SerializeField] private LevelFaсtory _levelFactory;

        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private Image _image; 
        
        private int _maxEnemies;

        private void Start()
        {
            _levelFactory.EnemyCountChanged += EnemyCount;
            _levelFactory.WaveStarted += OnWaveStarted;
            _levelFactory.WaveEnded += OnWaveEnded;
        }

        private void OnWaveEnded()
        {
            _textMesh.text = "";
            _image.enabled = false;
        }

        private void OnWaveStarted(int enemys)
        {
            _maxEnemies = enemys;
            _image.enabled = true;
            EnemyCount(enemys);
        }

        private void EnemyCount(int count)
        {
            _textMesh.text = count + " / " + _maxEnemies;
        }
    }
}
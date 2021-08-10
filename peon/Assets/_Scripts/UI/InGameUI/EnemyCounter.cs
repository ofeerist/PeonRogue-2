using _Scripts.Level;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.InGameUI
{
    public class EnemyCounter : MonoBehaviour
    {
        [SerializeField] private LevelFaсtory _levelFactory;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private int _maxEnemies;

        private void Start()
        {
            _levelFactory.EnemyCountChanged += EnemyCount;
            _levelFactory.WaveStarted += OnWaveStarted;
            _levelFactory.WaveEnded += OnWaveEnded;
        }

        private void OnWaveEnded() => _textMesh.text = "";

        private void OnWaveStarted(int enemys)
        {
            _maxEnemies = enemys;
            EnemyCount(enemys);
        }

        private void EnemyCount(int count)
        {
            _textMesh.text = count + " / " + _maxEnemies;
        }
    }
}
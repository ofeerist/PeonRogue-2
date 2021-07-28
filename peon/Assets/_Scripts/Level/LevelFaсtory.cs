using Game.Unit;
using UnityEngine;

namespace Game.Level
{
    class LevelFaсtory : MonoBehaviour
    {
        [SerializeField] private UnitHandler _unitHandler;

        [Space]

        private WaveInfo[] _waveInfos;
        private SpawnPosition[] _playerSpawnPositions;
        private RewardPosition[] _rewardPositions;
        private TransferPosition[] _transferPositions;

        public delegate void WaveStart(int enemys);
        public event WaveStart WaveStarted;

        public delegate void WaveEnd();
        public event WaveEnd WaveEnded;

        private int _currentWave = 0;
        private int _waveCount;

        private float _currentEnemyCount;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void Process()
        {
            var info = FindObjectOfType<LevelInformation>();
            _waveInfos = info.WaveInfos;
            _waveCount = _waveInfos.Length;
            _playerSpawnPositions = info.PlayerSpawnPositions;
            _rewardPositions = info.RewardPositions;
            _transferPositions = info.TransferPositions;

            for (int i = 0; i < _unitHandler.Units.Count; i++)
            {
                _unitHandler.Units[i].transform.position = _playerSpawnPositions[Random.Range(0, _playerSpawnPositions.Length)].GetPosition();
            }

            StartRandomWave();
        }

        private void StartRandomWave()
        {
            StartWave(Wave.Generate(_waveInfos[_currentWave].UnitData, 5 + _currentWave, _waveInfos[_currentWave].MaxUsages));
        }

        private void StartWave(Wave wave)
        {
            _currentWave++;

            _currentEnemyCount = wave.WaveEnemies.Count;
            for (int i = 0; i < wave.WaveEnemies.Count; i++)
            {
                var u = Instantiate(wave.WaveEnemies[i].Prefab, _playerSpawnPositions[Random.Range(0, _playerSpawnPositions.Length)].GetPosition(), Quaternion.identity);
                wave.WaveEnemies[i].SetData(u);
                u.GetComponent<EnemyHealth>().OnDeath += () => { _currentEnemyCount--; if (_currentEnemyCount == 0) EndWave(); };
            }
            
            WaveStarted?.Invoke(wave.WaveEnemies.Count);
        }

        private void EndWave()
        {
            WaveEnded?.Invoke();

            if (_currentWave >= _waveCount)
            {
                SpawnReward();
            }
            else 
            {
                StartRandomWave();
            }
        }

        private void SpawnReward()
        {
            ActivateTransfer();
        }

        private void ActivateTransfer()
        {

        }
    }
}

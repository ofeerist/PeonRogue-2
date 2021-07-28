using Game.Unit;
using UnityEngine;

namespace Game.Level
{
    class LevelFaсtory : MonoBehaviour
    {
        [SerializeField] private UnitHandler _unitHandler;

        [Space]

        private UnitData.UnitData[] _unitData;
        private SpawnPosition[] _playerSpawnPositions;
        private RewardPosition[] _rewardPositions;
        private TransferPosition[] _transferPositions;
        private MaxUsage[] _maxUsages;

        public delegate void WaveStart(int enemys);
        public event WaveStart WaveStarted;

        public delegate void WaveEnd();
        public event WaveEnd WaveEnded;

        private float _currentWave = 0;

        private float _currentEnemyCount;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void Process()
        {
            var info = FindObjectOfType<LevelInformation>();
            _unitData = info.UnitData;
            _playerSpawnPositions = info.PlayerSpawnPositions;
            _rewardPositions = info.RewardPositions;
            _transferPositions = info.TransferPositions;
            _maxUsages = info.MaxUsages;

            for (int i = 0; i < _unitHandler.Units.Count; i++)
            {
                _unitHandler.Units[i].transform.position = _playerSpawnPositions[Random.Range(0, _playerSpawnPositions.Length)].GetPosition();
            }

            StartWave(Wave.Generate(_unitData, 5 + _currentWave, _maxUsages));
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
        }
    }
}

using _Scripts.Level.Interactable.Talants;
using _Scripts.Unit;
using UnityEngine;

namespace _Scripts.Level
{
    class LevelFaсtory : MonoBehaviour
    {
        [SerializeField] private UnitHandler _unitHandler;
        [SerializeField] private TalentWindow _talentWindow;

        [Space]

        private WaveInfo[] _waveInfos;
        private SpawnPosition[] _playerSpawnPositions;
        private SpawnPosition[] _enemySpawnPositions;
        private SpawnPosition[] _transferPositions;

        public delegate void WaveStart(int enemys);
        public event WaveStart WaveStarted;

        public delegate void WaveEnd();
        public event WaveEnd WaveEnded;

        private int _currentWave = 0;
        private int _waveCount;

        private int _currentEnemyCount;

        private int CurrentEnemyCount { get { return _currentEnemyCount; } set { _currentEnemyCount = value; EnemyCountChanged?.Invoke(value); } }

        public delegate void EnemyCount(int count);
        public event EnemyCount EnemyCountChanged;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        private void ApplyLevelData(LevelInformation info)
        {
            _waveInfos = info.WaveInfos;
            _waveCount = _waveInfos.Length;
            _playerSpawnPositions = info.PlayerSpawnPositions;
            _enemySpawnPositions = info.EnemySpawnPositions;
            _transferPositions = info.TransferPositions;
        }

        public void Process()
        {
            ApplyLevelData(FindObjectOfType<LevelInformation>());
            
            for (int i = 0; i < _unitHandler.Units.Count; i++)
                _unitHandler.Units[i].transform.position = _playerSpawnPositions[Random.Range(0, _playerSpawnPositions.Length)].GetPosition();
            
            StartRandomWave();
        }

        private void StartRandomWave()
        {
            StartWave(Wave.Generate(_waveInfos[_currentWave].UnitData, 5 + _currentWave, _waveInfos[_currentWave].MaxUsages));
        }

        private void StartWave(Wave wave)
        {
            _currentWave++;

            CurrentEnemyCount = wave.WaveEnemies.Count;
            for (int i = 0; i < wave.WaveEnemies.Count; i++)
            {
                var u = Instantiate(wave.WaveEnemies[i].Prefab, _enemySpawnPositions[Random.Range(0, _enemySpawnPositions.Length)].GetPosition(), Quaternion.identity);
                wave.WaveEnemies[i].SetData(u);
                u.GetComponent<UnitHealth>().OnDeath += EnemyDeath;
            }
            
            WaveStarted?.Invoke(wave.WaveEnemies.Count);
        }

        private void EnemyDeath(Unit.Unit u)
        {
            CurrentEnemyCount--;
            if (CurrentEnemyCount == 0) EndWave(u);
        }

        private void EndWave(Unit.Unit u)
        {
            WaveEnded?.Invoke();

            if (_currentWave >= _waveCount)
            {
                SpawnReward(u);
            }
            else 
            {
                StartRandomWave();
            }
        }

        private void SpawnReward(Unit.Unit u)
        {
            _talentWindow.Add(Talents.Thrall);
            ActivateTransfer();
        }

        private void ActivateTransfer()
        {

        }
    }
}

using _Scripts.Level.Interactable;
using _Scripts.Level.Interactable.Talents;
using _Scripts.Unit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Level
{
    class LevelFaсtory : MonoBehaviour
    {
        [SerializeField] private UnitHandler _unitHandler;
        [SerializeField] private TalentWindow _talentWindow;
        [SerializeField] private LevelLoader _levelLoader;
        [SerializeField] private Transfer _transfer;
        [SerializeField] private Talent _talent;
        
        [Space]

        private WaveInfo[] _waveInfos;
        private SpawnPosition[] _playerSpawnPositions;
        private SpawnPosition[] _enemySpawnPositions;
        private SpawnPosition[] _transferPositions;
        private string[] _possibleScenes;
        
        public delegate void WaveStart(int enemys);
        public event WaveStart WaveStarted;

        public delegate void WaveEnd();
        public event WaveEnd WaveEnded;

        private int _currentWave = 0;
        private int _waveCount;

        private int _currentEnemyCount;

        private int CurrentEnemyCount { get => _currentEnemyCount;
            set { _currentEnemyCount = value; EnemyCountChanged?.Invoke(value); } }

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
            _possibleScenes = info.PossibleScenes;
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
            foreach (var t in wave.WaveEnemies)
            {
                var u = Instantiate(t.Prefab, _enemySpawnPositions[Random.Range(0, _enemySpawnPositions.Length)].GetPosition(), Quaternion.identity);
                t.SetData(u);
                u.GetComponent<UnitHealth>().OnDeath += EnemyDeath;
            }
            
            WaveStarted?.Invoke(wave.WaveEnemies.Count);
        }

        private void EnemyDeath(Unit.Unit u)
        {
            CurrentEnemyCount--;
            if (CurrentEnemyCount == 0) EndWave(u);
        }

        private void EndWave(Component u)
        {
            WaveEnded?.Invoke();

            if (_currentWave >= _waveCount)
            {
                SpawnReward(u);
                SpawnTransfers();
            }
            else 
            {
                StartRandomWave();
            }
        }

        private void SpawnTransfers()
        {
            foreach (var pos in _transferPositions)
            {
                var transfer = Instantiate(_transfer, pos.GetPosition(), Quaternion.identity);
                transfer.Scene = _possibleScenes[Random.Range(0, _possibleScenes.Length)];
                transfer.OnInteract += ActivateTransfer;
            }
        }

        private void SpawnReward(Component u)
        {
            Instantiate(_talent, u.transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
        }

        private void ActivateTransfer(Interactable.Interactable interactable)
        {
            if (!(interactable is Transfer transfer)) return;
            
            _levelLoader.LoadScene(transfer.Scene);
        }
    }
}

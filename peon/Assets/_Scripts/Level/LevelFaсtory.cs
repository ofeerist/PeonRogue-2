
using System;
using System.Collections;
using _Scripts.Level.Interactable;
using _Scripts.Level.Interactable.Talents;
using _Scripts.UI.InGameUI;
using _Scripts.Unit;
using _Scripts.Unit.AI;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Level
{
    public class LevelFaсtory : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private UnitHandler _unitHandler;
        [SerializeField] private UnitObserver _unitObserver;
        [SerializeField] private TalentWindow _talentWindow;
        [SerializeField] private LevelLoader _levelLoader;
        [SerializeField] private Transfer _transfer;
        [SerializeField] private Talent _talent;

        public UnitObserver UnitObserver => _unitObserver;

        [Space] 
        
        [SerializeField] private ParticleSystem _spawnEffect;
        
        [Space]

        private WaveInfo[] _waveInfos;
        private SpawnPosition[] _playerSpawnPositions;
        private SpawnPosition[] _enemySpawnPositions;
        private SpawnPosition[] _transferPositions;
        private string[] _possibleScenes;
        private Wave _wave;
        
        public delegate void WaveStart(int enemys);
        public event WaveStart WaveStarted;

        public delegate void WaveEnd();
        public event WaveEnd WaveEnded;

        private int _currentWave;
        private int _waveCount;

        private int _currentEnemyCount;
        
        private int CurrentEnemyCount { get => _currentEnemyCount;
            set { _currentEnemyCount = value; EnemyCountChanged?.Invoke(value); } }

        public delegate void EnemyCount(int count);
        public event EnemyCount EnemyCountChanged;

        private PhotonView _photonView;

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            
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
            
            var pos = _playerSpawnPositions[Random.Range(0, _playerSpawnPositions.Length)].GetPosition();

            GameInitilizer.CreatePlayerUnit(pos, _unitObserver, _unitHandler);
            
            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                StartRandomWave();
            });
        }

        private void StartRandomWave()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            _photonView.RPC(nameof(RPCStartRandomWave), RpcTarget.AllBufferedViaServer, Random.Range(0, 100000));

        }
        
        [PunRPC]
        private void RPCStartRandomWave(int seed)
        {
            _wave = Wave.Generate(_waveInfos[_currentWave].UnitData, 5 + _currentWave, _waveInfos[_currentWave].MaxUsages, seed);
            StartWave(_wave, seed);
        }
        
        private void StartWave(Wave wave, int seed)
        {
            _currentWave++;
            
            WaveStarted?.Invoke(wave.WaveEnemies.Count);
            CurrentEnemyCount = wave.WaveEnemies.Count;

            SpawnEffect(wave, seed);
        }

        private void SpawnEffect(Wave wave, int seed)
        {
            var effs = new ParticleSystem[wave.WaveEnemies.Count];
            
            var r = new System.Random(seed);

            for (int i = 0; i < wave.WaveEnemies.Count; i++)
            {
                var pos = _enemySpawnPositions[r.Next(0, _enemySpawnPositions.Length)].GetPosition(r);
                effs[i] = Instantiate(_spawnEffect,
                    pos,
                    Quaternion.identity);

                effs[i].Play();
            }

            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                SpawnEnemies(wave, seed);

                foreach (var e in effs)
                    e.Stop();

                _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(x =>
                {
                    foreach (var e in effs)
                        Destroy(e.gameObject);
                });
            });


        }
        
        private void SpawnEnemies(Wave wave, int seed)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            var r = new System.Random(seed);
            for (int i = 0; i < wave.WaveEnemies.Count; i++)
            {
                var pos = _enemySpawnPositions[r.Next(0, _enemySpawnPositions.Length)].GetPosition(r);
                var u = PhotonNetwork.Instantiate(wave.WaveEnemies[i].Prefab.name,
                        pos,
                        Quaternion.identity, 0, new object[]{i});
                u.GetComponent<AIHealth>().OnDeath += EnemyDeath;
            }
        }

        private void EnemyDeath(Unit.Unit u)
        {
            CurrentEnemyCount--;
            _photonView.RPC(nameof(ChangeCurrentEnemy), RpcTarget.Others, CurrentEnemyCount);
            
            if (CurrentEnemyCount == 0) EndWave(u);
        }

        [PunRPC]
        private void ChangeCurrentEnemy(int enemies)
        {
            CurrentEnemyCount = enemies;
        }
        
        private void EndWave(Component u)
        {
            WaveEnded?.Invoke();

            if (_currentWave >= _waveCount)
            {
                //SpawnReward(u);
                SpawnTransfers();
                Reset();
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
                var transfer = PhotonNetwork.Instantiate(_transfer.name, pos.GetPosition(), Quaternion.identity).GetComponent<Transfer>();
                transfer.Scene = _possibleScenes[Random.Range(0, _possibleScenes.Length)];
                transfer.OnInteract += ActivateTransfer;
            }
        }

        private void SpawnReward(Component u)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.Instantiate(_talent.name, u.transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
        }

        private void ActivateTransfer(Interactable.Interactable interactable)
        {
            if (!(interactable is Transfer transfer)) return;

            _levelLoader.LoadScene(transfer.Scene);
        }

        private void Reset()
        {
            _currentWave = 0;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            var u = info.photonView.gameObject.GetComponent<Unit.Unit>();
            int i = (int)info.photonView.InstantiationData[0];

            _wave.WaveEnemies[i].SetData(u);
        }
    }
}

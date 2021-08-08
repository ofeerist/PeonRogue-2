using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Level.Interactable;
using _Scripts.Level.Interactable.Talents;
using _Scripts.UI.InGameUI;
using _Scripts.Unit;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Event = _Scripts.Unit.Event;
using Random = UnityEngine.Random;

namespace _Scripts.Level
{
    class LevelFaсtory : MonoBehaviour
    {
        [SerializeField] private UnitHandler _unitHandler;
        [SerializeField] private UnitObserver _unitObserver;
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

        private Wave _wave;
        
        private int _currentEnemyCount;
        
        private int CurrentEnemyCount { get => _currentEnemyCount;
            set { _currentEnemyCount = value; EnemyCountChanged?.Invoke(value); } }

        public delegate void EnemyCount(int count);
        public event EnemyCount EnemyCountChanged;

        private PhotonView _photonView;
        
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

            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(3);
            StartRandomWave();
        }

        private void StartRandomWave()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            _photonView.RPC(nameof(RPCStartRandomWave), RpcTarget.AllBufferedViaServer, Random.Range(0, 100000));

        }
        
        [PunRPC]
        private void RPCStartRandomWave(int seed)
        {
            var wave = Wave.Generate(_waveInfos[_currentWave].UnitData, 5 + _currentWave, _waveInfos[_currentWave].MaxUsages, seed);
            StartWave(wave);
        }
        
        private void StartWave(Wave wave)
        {
            _wave = wave;
            
            _currentWave++;

            CurrentEnemyCount = wave.WaveEnemies.Count;

            if (PhotonNetwork.IsMasterClient)
                SpawnEnemies();
        }

        [PunRPC]
        private void SpawnEnemies()
        {
            if (PhotonNetwork.IsMasterClient) return;
            
            var r = new System.Random();
            foreach (var t in _wave.WaveEnemies)
            {
                var u = PhotonNetwork.Instantiate(t.Prefab.name,
                        _enemySpawnPositions[r.Next(0, _enemySpawnPositions.Length)].GetPosition(),
                        Quaternion.identity)
                    .GetComponent<Unit.Unit>();
                t.SetData(u);
                u.GetComponent<UnitHealth>().OnDeath += EnemyDeath;
            }

            _photonView.RPC(nameof(WaveStarting), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void WaveStarting()
        {
            WaveStarted?.Invoke(_wave.WaveEnemies.Count);
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
    }
}

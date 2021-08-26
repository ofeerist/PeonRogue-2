using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.UI;
using _Scripts.UI.InGameUI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Event = _Scripts.Unit.Event;

namespace _Scripts.Level
{
    internal class LevelLoader : MonoBehaviour
    {
        private int _loaded;
        private int Loaded { get => _loaded;
            set { _loaded = value; Changed(); } }

        private bool _started;

        private PhotonView _photonView;

        [SerializeField] private TextMeshProUGUI _textMesh;
        
        [SerializeField] private LevelFaсtory _levelFactory;
        [SerializeField] private DarknessTransition _darkness;

        [Space]

        [SerializeField] private GameObject _UI;
        [SerializeField] private Camera _loadingCamera;

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        public void LoadScene(string sceneName)
        {
            Loaded = 0;
            _started = false;
            
            _textMesh.text = "Loading...";
            
            _UI.SetActive(true);
            _textMesh.enabled = true;
            
            _darkness.Speed = 2f;
            _darkness.ActivateDark();
            
            _serialDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                _loadingCamera.gameObject.SetActive(true);
                if (PhotonNetwork.IsMasterClient) SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                SceneManager.sceneLoaded += ActivateReady;
            });
        }

        private void Changed()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                _started = true;
                _photonView.RPC(nameof(GameStart), RpcTarget.AllViaServer);
                return;
            }
            
            _textMesh.text = "Waiting for other players ..." + "(" + Loaded + "/" + PhotonNetwork.CurrentRoom.PlayerCount + ")";

            if (!PhotonNetwork.IsMasterClient) return;

            if (Loaded >= PhotonNetwork.CurrentRoom.PlayerCount && !_started)
            {
                _started = true;
                _photonView.RPC(nameof(GameStart), RpcTarget.AllViaServer);
            }
        }

        [PunRPC]
        private void GameStart()
        {
            _textMesh.enabled = false;

            _loadingCamera.gameObject.SetActive(false);
            _darkness.Speed = 2;
            _darkness.DeactivateDark();

            _levelFactory.Process();
        }

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            DontDestroyOnLoad(this);
        }

        private void ActivateReady(Scene scene, LoadSceneMode loadSceneMode)
        {
            _photonView.RPC(nameof(Ready), RpcTarget.AllBufferedViaServer);
        }

        [PunRPC]
        private void Ready()
        {
            Loaded++;
        }
    }
}

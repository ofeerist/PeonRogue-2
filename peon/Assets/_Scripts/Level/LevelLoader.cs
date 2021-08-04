using System.Collections;
using System.Collections.Generic;
using _Scripts.UI;
using _Scripts.UI.InGameUI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Event = _Scripts.Unit.Event;

namespace _Scripts.Level
{
    internal class LevelLoader : MonoBehaviour, IOnEventCallback
    {
        private int _loaded;
        private int Loaded { get => _loaded;
            set { _loaded = value; Changed(); } }

        private PhotonView _photonView;

        [SerializeField] private TextMeshProUGUI _textMesh;
        
        [SerializeField] private LevelFaсtory _levelFactory;
        [SerializeField] private DarknessTransition _darkness;

        [Space]

        [SerializeField] private GameObject _UI;
        [SerializeField] private Camera _loadingCamera;

        public void LoadScene(string sceneName)
        {
            Loaded = 0;
            _textMesh.text = "Loading...";
            
            _UI.SetActive(true);
            _darkness.Speed = 2f;
            _darkness.ActivateDark();

            StartCoroutine(Load(sceneName));
        }

        private IEnumerator Load(string sceneName)
        {
            yield return new WaitForSeconds(3f);
            
            _loadingCamera.gameObject.SetActive(true);
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            SceneManager.sceneLoaded += ActivateReady;
        }
        
        private void Changed()
        {
            _textMesh.text = "Waiting for other players ..." + "(" + Loaded + "/" + PhotonNetwork.CountOfPlayers + ")";

            if (!PhotonNetwork.IsMasterClient) return;

            if (Loaded >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                GameStart();
            }
        }

        private static void GameStart()
        {
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)Event.GameStart, null, options, sendOptions);
        }

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            DontDestroyOnLoad(this);
        }

        private void ActivateReady(Scene scene, LoadSceneMode loadSceneMode)
        {
            _photonView.RPC(nameof(Ready), RpcTarget.AllBufferedViaServer);
            SceneManager.sceneLoaded -= ActivateReady;
        }

        [PunRPC]
        private void Ready()
        {
            Loaded++;
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case (byte)Event.GameStart:
                    _textMesh.enabled = false;

                    _loadingCamera.gameObject.SetActive(false);
                    _darkness.Speed = 2;
                    _darkness.DeactivateDark();

                    _levelFactory.Process();
                    break;
            }
        }
        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}

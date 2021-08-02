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
    class LevelLoader : MonoBehaviour, IOnEventCallback
    {
        private int _loaded;
        private int Loaded { get { return _loaded; } set { _loaded = value; Changed(); } }

        private PhotonView _photonView;

        [SerializeField] private TextMeshProUGUI _textMesh;

        [SerializeField] private UnitHandler _unitHandler;
        [SerializeField] private UnitObserver _unitObserver;
        [SerializeField] private LevelFaсtory _levelFactory;
        [SerializeField] private DarknessTransition _darkness;

        [Space]

        [SerializeField] private GameObject _UI;
        [SerializeField] private Camera _loadingCamera;

        public void LoadScene(string sceneName)
        {
            _loadingCamera.gameObject.SetActive(true);
            _UI.SetActive(true);
            _darkness.ActivateDarkImmediatly();

            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            SceneManager.sceneLoaded += ActivateReady;
        }

        private void Changed()
        {
            _textMesh.text = "Waiting for other players ..." + "(" + Loaded + "/" + PhotonNetwork.CountOfPlayers + ")";

            if (!PhotonNetwork.IsMasterClient) return;

            if (Loaded == PhotonNetwork.CountOfPlayers)
            {
                GameStart();
            }
        }

        private void GameStart()
        {
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
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
                    GameInitilizer.CreatePlayerUnit(_photonView, transform, _unitObserver, _unitHandler);

                    _textMesh.enabled = false;

                    _loadingCamera.gameObject.SetActive(false);
                    _darkness.Speed = 2;
                    _darkness.DeactivateDark();

                    _levelFactory.Process();
                    break;

                default:
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

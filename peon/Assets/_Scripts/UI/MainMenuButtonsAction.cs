using System;
using System.Collections;
using _Scripts.Unit.Doodads;
using Cinemachine;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class MainMenuButtonsAction : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain _cinemachineBrain;
        [SerializeField] private GameStarter _gameStarter;

        [Space]

        [SerializeField] private Button _singlePlayer;
        [SerializeField] private Button _multiPlayer;
        [SerializeField] private Button _options;
        [SerializeField] private Button _quit;

        [Space]

        [SerializeField] private GameObject _buttonsPanel;
        [SerializeField] private GameObject _multiPlayerPanel;
        [SerializeField] private GameObject _optionsPanel;

        [Space]

        [SerializeField] private DarknessTransition _darkness;
        [SerializeField] private Door _door;

        [Space]

        [SerializeField] private CinemachineVirtualCamera _mainCamera;
        [SerializeField] private CinemachineVirtualCamera _multiplayerCamera;
        [SerializeField] private CinemachineVirtualCamera _optionsCamera;

        [Space]

        [SerializeField] private Transform _peonTransform;
        [SerializeField] private Transform _defaultPeonTransform;
        [SerializeField] private Transform _optionsPeonTransform;

        private readonly SerialDisposable _changeDisposable = new SerialDisposable();
        private readonly SerialDisposable _transferDisposable = new SerialDisposable();
        
        private void Awake()
        {
            _changeDisposable.AddTo(this);
            _transferDisposable.AddTo(this);
        }

        private void Start()
        {
            DisableAllCameras();
            _mainCamera.gameObject.SetActive(true);

            _singlePlayer.onClick.AddListener(SinglePlayer);
            _multiPlayer.onClick.AddListener(MultiPlayer);
            _options.onClick.AddListener(Options);
            _quit.onClick.AddListener(Quit);
        }

        public void MainMenu()
        {
            ChangePeonPos(true);
            TransferTo(_mainCamera, _buttonsPanel, false);
        }

        private void SinglePlayer()
        {
            PhotonNetwork.OfflineMode = true;

            _gameStarter.StartSingleGame();
        }

        private void MultiPlayer()
        {
            ChangePeonPos(true);
            TransferTo(_multiplayerCamera, _multiPlayerPanel, true);
        }

        private void Options()
        {
            ChangePeonPos(false);
            TransferTo(_optionsCamera, _optionsPanel, false);
        }

        private void ChangePeonPos(bool def)
        {
            _changeDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.9f)).Subscribe(x =>
            {
                if (def)
                    _peonTransform.SetPositionAndRotation(_defaultPeonTransform.position,
                        _defaultPeonTransform.rotation);
                else
                    _peonTransform.SetPositionAndRotation(_optionsPeonTransform.position,
                        _optionsPeonTransform.rotation);
            });
        }

        private void TransferTo(Component cm, GameObject panel, bool door)
        {
            DisableAllPanels();

            if (door) _door.Open();
            else _door.Close();

            _darkness.Speed = 5;
            _darkness.ActivateDark();

            DisableAllCameras();
            cm.gameObject.SetActive(true);

            _transferDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(x =>
            {
                _darkness.Speed = 1;
                _darkness.DeactivateDark();

                panel.SetActive(true);
            });
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void DisableAllCameras()
        {
            _mainCamera.gameObject.SetActive(false);
            _multiplayerCamera.gameObject.SetActive(false);
            _optionsCamera.gameObject.SetActive(false);
        }

        private void DisableAllPanels()
        {
            _buttonsPanel.SetActive(false);
            _optionsPanel.SetActive(false);
            _multiPlayerPanel.SetActive(false);
        }
    }
}
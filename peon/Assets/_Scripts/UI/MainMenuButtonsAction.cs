using Cinemachine;
using Game.Unit;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuButtonsAction : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain _cinemachineBrain;

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
            StartCoroutine(ChangePeonPos(true));
            StartCoroutine(TransferTo(_mainCamera, _buttonsPanel, false));
        }

        private void SinglePlayer()
        {

        }

        private void MultiPlayer()
        {
            StartCoroutine(ChangePeonPos(true));
            StartCoroutine(TransferTo(_multiplayerCamera, _multiPlayerPanel, true));
        }

        private void Options()
        {
            StartCoroutine(ChangePeonPos(false));
            StartCoroutine(TransferTo(_optionsCamera, _optionsPanel, false));
        }

        private IEnumerator ChangePeonPos(bool def)
        {
            yield return new WaitForSeconds(0.9f);
            if(def) _peonTransform.SetPositionAndRotation(_defaultPeonTransform.position, _defaultPeonTransform.rotation);
            else _peonTransform.SetPositionAndRotation(_optionsPeonTransform.position, _optionsPeonTransform.rotation);
        }

        private IEnumerator TransferTo(CinemachineVirtualCamera camera, GameObject panel, bool door)
        {
            DisableAllPanels();

            if (door) _door.Open();
            else _door.Close();

            _darkness.Speed = 5;
            _darkness.ActivateDark();

            DisableAllCameras();
            camera.gameObject.SetActive(true);

            yield return new WaitForSeconds(1f);

            _darkness.Speed = 1;
            _darkness.DeactivateDark();

            panel.SetActive(true);
        }

        private void Quit()
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
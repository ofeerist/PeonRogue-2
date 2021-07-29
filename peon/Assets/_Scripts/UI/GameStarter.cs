using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Game.Level;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game.UI
{
    class GameStarter : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private DarknessTransition _darkness;

        private void Start()
        {
            if (_startButton == null) return;

            _startButton.onClick.AddListener(() => {
                GetComponent<PhotonView>().RPC(nameof(StartGame), RpcTarget.All);
            });

            if (PhotonNetwork.MasterClient != PhotonNetwork.LocalPlayer) _startButton.gameObject.SetActive(false);
        }

        [PunRPC]
        private void StartGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            if (PhotonNetwork.MasterClient == PhotonNetwork.LocalPlayer)
                StartCoroutine(LoadLevel());
        }

        public void StartSingleGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();
            StartCoroutine(LoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            _darkness.Speed = 1;
            _darkness.ActivateDark();

            yield return new WaitForSeconds(3f);
            FindObjectOfType<LevelLoader>().LoadScene("FirstLocation");
        }
    }
}

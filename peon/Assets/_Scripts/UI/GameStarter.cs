using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Cinemachine;
using Game.Unit;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace Game.UI
{
    class GameStarter : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private DarknessTransition _darkness;

        private void Start()
        {
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
                StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            yield return new WaitForSeconds(3f);
            
            SceneManager.LoadSceneAsync("FirstLocation", LoadSceneMode.Single);
        }
    }
}

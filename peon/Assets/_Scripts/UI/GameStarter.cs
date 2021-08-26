using System;
using _Scripts.Level;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private DarknessTransition _darkness;

        private void Start()
        {
            if (_startButton == null) return;

            _startButton.onClick.AddListener(() => {
                GetComponent<PhotonView>().RPC(nameof(StartGame), RpcTarget.All);
            });

            if (!PhotonNetwork.IsMasterClient) _startButton.gameObject.SetActive(false);
        }

        [PunRPC]
        private void StartGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            
            Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                FindObjectOfType<LevelLoader>().LoadScene("FirstLocation");
            }).AddTo(_darkness);
        }

        public void StartSingleGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();
            
            Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(x =>
            {
                FindObjectOfType<LevelLoader>().LoadScene("FirstLocation");
            }).AddTo(_darkness);
        }
    }
}

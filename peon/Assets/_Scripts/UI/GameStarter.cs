using System.Collections;
using _Scripts.Level;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
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

            if (!PhotonNetwork.IsMasterClient) _startButton.gameObject.SetActive(false);
        }

        [PunRPC]
        private void StartGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            
            _darkness.StartCoroutine(LoadLevel());
        }

        public void StartSingleGame()
        {
            _darkness.Speed = 2;
            _darkness.ActivateDark();
            StartCoroutine(LoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            yield return new WaitForSeconds(3f);
            FindObjectOfType<LevelLoader>().LoadScene("FirstLocation");
        }
    }
}

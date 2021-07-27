using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

namespace Game.Level
{
    class ReadyWaiter : MonoBehaviour
    {
        private int _loaded;
        private int Loaded { get { return _loaded; } set { _loaded = value; Changed(); } }

        private void Changed()
        {
            _textMesh.text = "Waiting for other players ..." + "(" + Loaded + "/" + PhotonNetwork.CountOfPlayers + ")";
        }

        private PhotonView _photonView;

        [SerializeField] private TextMeshProUGUI _textMesh;

        private IEnumerator Start()
        {
            _photonView = GetComponent<PhotonView>();

            yield return new WaitForSeconds(.5f);
            SceneManager.LoadSceneAsync("FirstLocation", LoadSceneMode.Additive);
            SceneManager.sceneLoaded += ActivateReady;
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (Loaded == PhotonNetwork.CountOfPlayers) 
            {
                FindObjectOfType<GameInitilizer>().GameInit();
            }
                
        }

        private void ActivateReady(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != "Waiting")
            _photonView.RPC(nameof(Ready), RpcTarget.AllBufferedViaServer);
        }

        [PunRPC]
        private void Ready()
        {
            Loaded++;
        }
    }
}

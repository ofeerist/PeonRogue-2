using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    class GameCreator : MonoBehaviour
    {
        [SerializeField] Button _createButton;
        [SerializeField] TMP_InputField _gameName;

        private void Start()
        {
            _createButton.onClick.AddListener(CreateGame);
        }
        private void CreateGame()
        {
            var roomOptions = new RoomOptions() { MaxPlayers = 6 };
            _gameName.text = string.IsNullOrEmpty(_gameName.text) ? "Peon " + Random.Range(1, 1000) : _gameName.text;
            _gameName.text = _gameName.text.Length > 20 ? _gameName.text.Substring(0, 20) : _gameName.text;
            PhotonNetwork.CreateRoom(_gameName.text, roomOptions);
        }
    }
}

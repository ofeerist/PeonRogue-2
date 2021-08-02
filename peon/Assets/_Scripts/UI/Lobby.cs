using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    class Lobby : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _gameName;
        public TextMeshProUGUI GameName { get { return _gameName; } }

        [SerializeField] private TextMeshProUGUI _slotsCount;
        public TextMeshProUGUI SlotsCount { get { return _slotsCount; } }

        [SerializeField] private Button _button;

        private void Start()
        {
            _button.onClick.AddListener(JoinGame);
        }

        private void JoinGame()
        {
            PhotonNetwork.JoinRoom(GameName.text);
        }
    }
}

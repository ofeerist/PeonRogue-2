using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Colorizing;

namespace Game.UI
{
    class LobbyChat : MonoBehaviourPun
    {
        [SerializeField] private TMP_InputField _messageInputField;
        [SerializeField] private Button _sendMessageButton;
        [Space]
        [SerializeField] private Transform _messageGrid;
        [SerializeField] private TextMeshProUGUI _message;
        [Space]
        [SerializeField] private ScrollRect _scrollRect;
        [Space]
        [SerializeField] private PhotonView _photonView;

        private void Start()
        {
            _sendMessageButton.onClick.AddListener(() => 
            { 
                SendMsg(СoncatenationPlayerMessage(_messageInputField.text));
            });
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == _messageInputField.gameObject &&
                (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
            {
                _sendMessageButton.onClick.Invoke();
            }
        }

        private string СoncatenationPlayerMessage(string message)
        {
            var color = TextTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
            return "<color=" + color + ">" + PhotonNetwork.LocalPlayer.NickName + "</color>" + ": " + message;
        }

        public void SendMsg(string message, bool system = false)
        {
            if (!system)
            {
                if (_messageInputField.text.Length > 0 && _messageInputField.text.Length < 500)
                {
                    _photonView.RPC(nameof(SendMessage), RpcTarget.All, PhotonNetwork.LocalPlayer, message);
                    _messageInputField.text = "";
                }

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_messageInputField.gameObject);
            }
            else
            {
                _photonView.RPC(nameof(SendMessage), RpcTarget.All, PhotonNetwork.LocalPlayer, message);
            }
        }

        [PunRPC]
        private void SendMessage(Player sender, string message)
        {
            var msg = Instantiate(_message, _messageGrid);
            msg.text = message;
            StartCoroutine(SetScrollO());
        }

        private IEnumerator SetScrollO()
        {
            yield return 0;
            _scrollRect.verticalNormalizedPosition= 0;
        }

        public void ClearMessages()
        {
            for (int i = 0; i < _messageGrid.childCount; i++)
                Destroy(_messageGrid.GetChild(i).gameObject);
        }
    }
}

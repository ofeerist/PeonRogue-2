using System;
using System.Collections;
using _Scripts.Color;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
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

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _sendMessageButton.onClick.AddListener(() => 
            { 
                SendMsg(СoncatenationPlayerMessage(_messageInputField.text));
            });

            Observable.EveryUpdate().Subscribe(x =>
            {
                if (EventSystem.current.currentSelectedGameObject == _messageInputField.gameObject &&
                    (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
                {
                    _sendMessageButton.onClick.Invoke();
                }
            }).AddTo(this);
        }

        private static string СoncatenationPlayerMessage(string message)
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
                    _photonView.RPC(nameof(SndMessage), RpcTarget.All, message);
                    _messageInputField.text = "";
                }

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_messageInputField.gameObject);
            }
            else
            {
                _photonView.RPC(nameof(SndMessage), RpcTarget.All, message);
            }
        }

        [PunRPC]
        private void SndMessage(string message)
        {
            var msg = Instantiate(_message, _messageGrid);
            msg.text = message;

            _serialDisposable.Disposable = Observable.NextFrame().Subscribe(x =>
            {
                _scrollRect.verticalNormalizedPosition= 0;
            });
        }

        public void ClearMessages()
        {
            for (int i = 0; i < _messageGrid.childCount; i++)
                Destroy(_messageGrid.GetChild(i).gameObject);
        }
    }
}

using _Scripts.Color;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    class LobbyCallbacks : MonoBehaviourPunCallbacks
    {
        [SerializeField] private LobbyChat _chat;
        [SerializeField] private PreviewPeons _previewPeons;
        [SerializeField] private PhotonView _photonView;
        [SerializeField] private Button _startButton;

        [PunRPC]
        private void UpdatePreviewPeonsColor(int i, float r, float g, float b, float a)
        {
            _previewPeons.Peons[i].MeshRenderer.material.SetColor("TeamColor", new UnityEngine.Color(r, g, b, a));
        }

        [PunRPC]
        private void UpdatePreviewPeons()
        {
            for (int i = 0; i < _previewPeons.Peons.Length; i++)
            {
                _previewPeons.Peons[i].GameObject.SetActive(false);
            }
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == null) continue;

                _previewPeons.Peons[i].TextName.text = PhotonNetwork.PlayerList[i].NickName;
                _previewPeons.Peons[i].GameObject.SetActive(true);

                if (PhotonNetwork.LocalPlayer != PhotonNetwork.PlayerList[i]) continue;

                var color = ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
                _photonView.RPC(nameof(UpdatePreviewPeonsColor), RpcTarget.All, i, color.r, color.g, color.b, color.a);
            }
        }

        [PunRPC]
        private void UpdatePreviewPeonsOnLeave(Player leaver)
        {
            for (int i = 0; i < _previewPeons.Peons.Length; i++)
            {
                _previewPeons.Peons[i].GameObject.SetActive(false);
            }
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                _previewPeons.Peons[i].GameObject.SetActive(false);

                if (PhotonNetwork.PlayerList[i] == null && PhotonNetwork.PlayerList[i] != leaver) continue;

                _previewPeons.Peons[i].TextName.text = PhotonNetwork.PlayerList[i].NickName;
                _previewPeons.Peons[i].GameObject.SetActive(true);

                if (PhotonNetwork.LocalPlayer != PhotonNetwork.PlayerList[i]) continue;

                var color = ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
                _photonView.RPC(nameof(UpdatePreviewPeonsColor), RpcTarget.All, i, color.r, color.g, color.b, color.a);
            }
        }

        private void UpdatePeonOnSelfLeave()
        {
            for (int i = 0; i < _previewPeons.Peons.Length; i++)
            {
                _previewPeons.Peons[i].GameObject.SetActive(false);
            }

            _previewPeons.Peons[0].GameObject.SetActive(true);
            _previewPeons.Peons[0].TextName.text = PhotonNetwork.NickName;
            _previewPeons.Peons[0].MeshRenderer.material.SetColor("TeamColor", ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color)));
        }

        public override void OnJoinedRoom()
        {
            _photonView.RPC(nameof(UpdatePreviewPeons), RpcTarget.All);

            _chat.SendMsg(PhotonNetwork.LocalPlayer.NickName + " <color=#ffcc00>joined", true);
        }

        public override void OnLeftRoom()
        {
            UpdatePeonOnSelfLeave(); 
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

            _chat.SendMsg(otherPlayer.NickName + " <color=#ffcc00>left", true);
            _photonView.RPC(nameof(UpdatePreviewPeonsOnLeave), RpcTarget.All, otherPlayer);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                _photonView.RPC(nameof(UpdatePreviewPeons), RpcTarget.All);
                _startButton.gameObject.SetActive(true);
            }
        }
    }
}

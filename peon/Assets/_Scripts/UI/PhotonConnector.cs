using _Scripts.Color;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    class PhotonConnector : MonoBehaviourPunCallbacks
    {
        [SerializeField] private PreviewPeons _previewPeons;
        [SerializeField] private Button _button;
        [SerializeField] private DarknessTransition _darkness;
        private static readonly int TeamColor = Shader.PropertyToID("TeamColor");

        private void Start()
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString(PrefsConstants.Nickname) ?? SystemInfo.deviceName;

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "beta1";

            PhotonNetwork.ConnectUsingSettings();

            var asource = GetComponent<AudioSource>();
            asource.clip = Resources.Load<AudioClip>("Sound/UI/claninvitation");
            asource.Play();

            _darkness.DeactivateDark();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby(new TypedLobby("PeonRogue " + PhotonNetwork.GameVersion, LobbyType.Default));
            _button.interactable = true;

            _previewPeons.Peons[0].TextName.text = PlayerPrefs.GetString(PrefsConstants.Nickname) ?? SystemInfo.deviceName;
            _previewPeons.Peons[0].MeshRendereres[0].material.SetColor(TeamColor, ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color)));
            _previewPeons.Peons[0].MeshRendereres[1].material.SetColor(TeamColor, ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color)));
        }
    }
}

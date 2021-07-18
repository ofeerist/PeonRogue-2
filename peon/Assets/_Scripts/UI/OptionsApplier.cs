using UnityEngine;
using TMPro;
using Photon.Pun;
using Game.Colorizing;

namespace Game.UI
{
    class OptionsApplier : MonoBehaviour
    {
        [SerializeField] private TMP_InputField NicknameInput;
        [SerializeField] private TextMeshProUGUI NicknameText;

        [SerializeField] private TMP_Dropdown ColorInput;

        [SerializeField] private PreviewPeons _previewPeons;
        private void Start()
        {
            NicknameText.text = PlayerPrefs.GetString(PrefsConstants.Nickname) ?? SystemInfo.deviceName;
            ColorInput.value = PlayerPrefs.GetInt(PrefsConstants.Color);

            NicknameInput.onValueChanged.AddListener((s) => {
                PlayerPrefs.SetString(PrefsConstants.Nickname,
                    s.Length > 0 && s.Length <= 20 && !string.IsNullOrEmpty(s) ? s : SystemInfo.deviceName);
                s = PlayerPrefs.GetString(PrefsConstants.Nickname);

                PhotonNetwork.NickName = s;
                NicknameText.text = s;

                _previewPeons.Peons[0].TextName.text = PlayerPrefs.GetString(PrefsConstants.Nickname) ?? SystemInfo.deviceName;
            });

            ColorInput.onValueChanged.AddListener((i) => {
                PlayerPrefs.SetInt(PrefsConstants.Color, i);
                _previewPeons.Peons[0].MeshRenderer.material.SetColor("TeamColor", ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color)));
            });
        }
    }
}

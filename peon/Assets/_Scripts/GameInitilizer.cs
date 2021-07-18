using Cinemachine;
using ExitGames.Client.Photon;
using Game.Colorizing;
using Game.UI.InGameUI;
using Game.Unit;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitilizer : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private GameObject _peon;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private Transform _referencePeon;

    private void Awake()
    {
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent((byte)Game.PhotonEvent.Event.GameStart, null, options, sendOptions);
    }

    private void StartGame()
    {
        gameObject.SetActive(false);

        var peon = PhotonNetwork.Instantiate(_peon.name, _referencePeon.position, _referencePeon.rotation);
        var color = ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
        var peonPhotonView = peon.GetPhotonView();
        GetComponent<PhotonView>().RPC(nameof(ChangePeonColor), RpcTarget.All, peonPhotonView.ViewID, color.r, color.g, color.b, color.a);

        var cameras = FindObjectsOfType<Camera>();
        foreach (var cam in cameras) cam.gameObject.SetActive(false);

        var camera = PhotonNetwork.Instantiate(_playerCamera.name, _referencePeon.position, _playerCamera.transform.rotation);
        var cinemaMachine = camera.GetComponentInChildren<CinemachineVirtualCamera>();
        cinemaMachine.Follow = peon.transform;
        cinemaMachine.LookAt = peon.transform;

        var movement = peon.GetComponent<PlayerMovement>();
        movement.MainCamera = camera.GetComponentInChildren<Camera>();

        var canvas = GameObject.Find("Canvas");
        var observer = FindObjectOfType(typeof(UnitObserver)) as UnitObserver;
        var unit = peon.GetComponent<Unit>();
        observer.Unit = unit;
    }

    [PunRPC]
    private void ChangePeonColor(int peonViewId, float r, float g, float b, float a)
    {
        var peon = PhotonNetwork.GetPhotonView(peonViewId).gameObject;
        var skinnedMesh = peon.GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMesh.material.SetColor("TeamColor", new Color(r, g, b, a));

        if (peon.GetPhotonView().Owner != PhotonNetwork.LocalPlayer) peon.GetComponentInChildren<Light>().gameObject.SetActive(false);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte)Game.PhotonEvent.Event.GameStart:
                StartGame();
                break;

            default:
                break;
        }
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}

using Cinemachine;
using ExitGames.Client.Photon;
using Game.Colorizing;
using Game.Level;
using Game.UI.InGameUI;
using Game.Unit;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitilizer
{
    public static void CreatePlayerUnit(PhotonView photonView, Transform referencePeon, UnitObserver unitObserver, UnitHandler unitHandler)
    {
        var peon = PhotonNetwork.Instantiate("Unit", referencePeon.position, referencePeon.rotation);
        var unit = peon.GetComponent<Unit>();
        var color = ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
        var peonPhotonView = peon.GetPhotonView();
        peonPhotonView.RPC("ChangeColor", RpcTarget.All, peonPhotonView.ViewID, color.r, color.g, color.b, color.a);

        unitHandler.Units.Add(unit);

        //var cameras = FindObjectsOfType<Camera>();
        //foreach (var cam in cameras) cam.gameObject.SetActive(false);

        var camera = PhotonNetwork.Instantiate("PlayerCamera", referencePeon.position, referencePeon.rotation);
        var cinemaMachine = camera.GetComponentInChildren<CinemachineVirtualCamera>();
        cinemaMachine.Follow = peon.transform;
        cinemaMachine.LookAt = peon.transform;

        var movement = peon.GetComponent<PlayerMovement>();
        movement.MainCamera = camera.GetComponentInChildren<Camera>();

        unitObserver.Unit = unit;
    }

}

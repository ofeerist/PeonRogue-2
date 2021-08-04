using System.Collections;
using _Scripts.Color;
using _Scripts.UI.InGameUI;
using _Scripts.Unit.Player;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Level
{
    public static class GameInitilizer
    {
        public static void CreatePlayerUnit(Vector3 position, UnitObserver unitObserver, UnitHandler unitHandler)
        {
            var rotation =
                Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            if (unitObserver.Unit == null)
            {
                var peon = PhotonNetwork.Instantiate("Unit", position, rotation);
                var unit = peon.GetComponent<Unit.Unit>();
                var color = ShaderTeamColor.ConvertColorNum(PlayerPrefs.GetInt(PrefsConstants.Color));
                var peonPhotonView = peon.GetPhotonView();
                peonPhotonView.RPC("ChangeColor", RpcTarget.All, peonPhotonView.ViewID, color.r, color.g, color.b, color.a);

                unitHandler.Units.Add(unit);

                var camera = PhotonNetwork.Instantiate("PlayerCamera", position, Quaternion.identity);
                var cinemaMachine = camera.GetComponentInChildren<CinemachineVirtualCamera>();
                cinemaMachine.Follow = peon.transform;
                cinemaMachine.LookAt = peon.transform;

                var movement = peon.GetComponent<PlayerMovement>();
                movement.MainCamera = camera.GetComponentInChildren<Camera>();

                unitObserver.Unit = unit;
                Object.DontDestroyOnLoad(unit);
                Object.DontDestroyOnLoad(camera);
            }
            else
            {
                unitObserver.StartCoroutine(Blyat(position, unitObserver));
            }
        }

        private static IEnumerator Blyat(Vector3 vpizdu, UnitObserver pizda)
        {
            yield return null;
            pizda.Unit.transform.SetPositionAndRotation(vpizdu, Quaternion.identity);
        }
    }
}

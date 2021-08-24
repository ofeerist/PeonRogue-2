using _Scripts.Color;
using _Scripts.UI.InGameUI;
using Cinemachine;
using KinematicCharacterController;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Level
{
    public static class GameInitilizer
    {
        private static readonly SerialDisposable _serialDisposable = new SerialDisposable();
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
                
                unit.Camera = camera.GetComponentInChildren<Camera>();

                unitObserver.Unit = unit;
                _serialDisposable.AddTo(unitObserver);

                _serialDisposable.Disposable = Observable.NextFrame().Subscribe(x =>
                {
                    peonPhotonView.RPC("DontDestroy", RpcTarget.All);
                });
            }
            else
            {
                var motor = unitObserver.Unit.GetComponent<KinematicCharacterMotor>();
                motor.SetPosition(position);

                motor.enabled = false;
                _serialDisposable.Disposable = Observable.NextFrame().Subscribe(x =>
                {
                    motor.enabled = true;
                });
            }
        }
    }
}

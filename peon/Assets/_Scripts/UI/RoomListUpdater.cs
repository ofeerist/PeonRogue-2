using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

namespace Game.UI
{
    class RoomListUpdater : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform _lobbyGrid;
        [SerializeField] private TextMeshProUGUI _noRoomsText;
        [SerializeField] private Lobby _lobby;
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _noRoomsText.text = "";
            for (int i = 0; i < _lobbyGrid.childCount; i++)
                Destroy(_lobbyGrid.GetChild(i).gameObject);
            
            foreach (var room in roomList)
            {
                if (!room.IsOpen || !room.IsVisible || room.MaxPlayers == 0) continue;

                var lobby = Instantiate(_lobby, _lobbyGrid);
                lobby.GameName.text = room.Name;
                lobby.SlotsCount.text = room.PlayerCount + "/" + room.MaxPlayers;
            }

            if (_lobbyGrid.childCount == 0) _noRoomsText.text = "No games hosted";
        }

    }
}

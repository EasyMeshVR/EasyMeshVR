using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using EasyMeshVR.UI;

namespace EasyMeshVR.Multiplayer
{
    public class RoomListMenu : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        [SerializeField]
        private GameObject roomEntryPrefab;

        [SerializeField]
        private GameObject roomListContent;

        private List<RoomEntry> roomEntries;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            roomEntries = new List<RoomEntry>();

            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonNetwork.InLobby)
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        #endregion

        #region Pun Callbacks

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo roomInfo in roomList)
            {
                Debug.Log("Room " + roomInfo.Name);

                if (!roomInfo.IsOpen)
                {
                    Debug.Log(roomInfo.Name + " is not open, not adding to room list");
                    continue;
                }

                GameObject roomEntryObject = Instantiate(roomEntryPrefab, roomListContent.transform);
                RoomEntry roomEntry = roomEntryObject.GetComponent<RoomEntry>();
                roomEntry.roomName = roomInfo.Name;
                roomEntry.maxPlayers = roomInfo.MaxPlayers;
                roomEntry.playerCount = roomInfo.PlayerCount;
                //roomEntries.Add(roomEntry);
            }
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using EasyMeshVR.UI;

namespace EasyMeshVR.Multiplayer
{
    public class RoomListMenu : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private GameObject roomEntryPrefab;

        [SerializeField]
        private GameObject roomListContent;

        private Dictionary<string, RoomEntry> roomEntries = new Dictionary<string, RoomEntry>();

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {

        }

        #endregion

        #region Public Functions

        public void OnJoinButtonClicked(string roomCode)
        {
            PhotonNetwork.JoinRoom(roomCode);
        }

        public void UpdateRoomlist(List<RoomInfo> roomList)
        {
            Debug.Log("Receieved room list of length " + roomList.Count);

            foreach (RoomInfo roomInfo in roomList)
            {
                Debug.Log("Room " + roomInfo.Name);

                if (!roomInfo.IsOpen || roomInfo.RemovedFromList)
                {
                    Debug.Log(roomInfo.Name + " is removed from list");
                    RoomEntry removedRoomEntry;

                    if (roomEntries.TryGetValue(roomInfo.Name, out removedRoomEntry) && removedRoomEntry)
                    {
                        Debug.Log("Removing it from the local room list");
                        Destroy(removedRoomEntry.gameObject);
                        roomEntries.Remove(roomInfo.Name);
                    }
                }
                else
                {
                    RoomEntry savedRoomEntry;
                    roomEntries.TryGetValue(roomInfo.Name, out savedRoomEntry);

                    // If room entry already instanatiated in our list, just update its player count
                    if (savedRoomEntry)
                    {
                        savedRoomEntry.playerCount = roomInfo.PlayerCount;
                    }
                    else
                    {
                        GameObject roomEntryObject = Instantiate(roomEntryPrefab, roomListContent.transform);
                        RoomEntry roomEntry = roomEntryObject.GetComponent<RoomEntry>();
                        roomEntry.roomName = roomInfo.Name;
                        roomEntry.playerCount = roomInfo.PlayerCount;
                        roomEntry.maxPlayers = roomInfo.MaxPlayers;
                        roomEntry.AddJoinButtonOnClickAction(() => PhotonNetwork.JoinRoom(roomInfo.Name));
                        roomEntries.Add(roomInfo.Name, roomEntry);
                    }
                }
            }
        }

        #endregion
    }
}

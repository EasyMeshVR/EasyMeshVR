using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

namespace EasyMeshVR.Multiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        [SerializeField]
        private GameObject launcherMenu;

        [SerializeField]
        private GameObject multiplayerMenu;

        [SerializeField]
        private GameObject roomListMenu;

        [SerializeField]
        private GameObject connectingPanel;

        [SerializeField]
        private TMP_InputField createRoomInputField;

        [SerializeField]
        private TMP_InputField joinRoomInputField;

        [SerializeField]
        private byte MAX_PLAYERS_PER_ROOM = 4;

        /// <summary>
        /// This client's game version number. Users can be separated from each other
        /// by gameVersion which allows you to make breaking changes.
        /// </summary>
        string gameVersion = "1.0.0";

        /// <summary>
        /// Keeps track of the current connection process. Since connection is asynchronous
        /// and is based on several callbacks from Photon, we need to keep track of this to
        /// properly adjust the behavior when we callback from Photon.
        /// </summary>
        bool isConnecting;

        /// <summary>
        /// Tracks whether or not the user is currently trying to create a room after clicking
        /// the "Create Room" button.
        /// </summary>
        bool creatingRoom;

        /// <summary>
        /// Keeps track of the room code that the user is currently trying to connect to.
        /// Reason for this is similar to isConnecting above.
        /// </summary>
        string roomCode;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            // This makes sure we can use PhotonNetwork.LoadLevel() on the master client and
            // all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        void Start()
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
            roomListMenu.SetActive(false);
            connectingPanel.SetActive(false);
        }

        #endregion

        #region Public Methods

        public void OnClickedSinglePlayer()
        {
            // Here we first turn PhotonNetwork.OfflineMode = true and then create
            // the offline room using Photon
            PhotonNetwork.OfflineMode = true;
            CreateSinglePlayerRoom();
        }

        public void OnClickedMultiPlayer()
        {
            launcherMenu.SetActive(false);
            multiplayerMenu.SetActive(true);
        }

        public void OnClickedRoomList()
        {
            multiplayerMenu.SetActive(false);
            roomListMenu.SetActive(true);
        }

        public void OnClickedBackMultiplayer()
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
        }

        public void OnClickedBackRoomList()
        {
            roomListMenu.SetActive(false);
            multiplayerMenu.SetActive(true);
        }

        public void OnClickedQuit()
        {
            Application.Quit();
        }

        public void OnClickedCreateRoom()
        {
            Debug.Log("Clicked create room button");

            if (string.IsNullOrWhiteSpace(createRoomInputField.text))
            {
                Debug.LogWarning("Can't create a room with an empty name.");
                return;
            }

            creatingRoom = true;
            Connect(createRoomInputField.text);
        }

        public void OnClickedJoinRoom()
        {
            Debug.Log("Clicked join room button");

            if (string.IsNullOrWhiteSpace(joinRoomInputField.text))
            {
                Debug.LogWarning("Invalid room name.");
                return;
            }

            creatingRoom = false;
            Connect(joinRoomInputField.text);
        }

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        public void Connect(string roomCode)
        {
            multiplayerMenu.SetActive(false);
            launcherMenu.SetActive(false);
            connectingPanel.SetActive(true);
            this.roomCode = roomCode;

            if (PhotonNetwork.IsConnected)
            {
                JoinOrCreateRoom();
            }
            else
            {
                isConnecting = PhotonNetwork.ConnectUsingSettings(); 
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public void JoinOrCreateRoom()
        {
            if (creatingRoom)
            {
                Debug.Log("Creating room " + roomCode);
                creatingRoom = false;
                PhotonNetwork.CreateRoom(roomCode, new RoomOptions
                {
                    MaxPlayers = MAX_PLAYERS_PER_ROOM
                });
            }
            else
            {
                Debug.Log("Joining room " + roomCode);
                PhotonNetwork.JoinRoom(roomCode);
            }
        }

        public void CreateSinglePlayerRoom()
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions {
                MaxPlayers = 1,
                IsVisible = false,
                IsOpen = false
            });
        }

        #endregion

        #region Pun Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected client to master server");

            // We don't want to do anything if we are not attempting to join/create a room.
            // The case where isConnecting is false is typically when you lost or quit the game,
            // when this level is loaded, OnConnectedToMaster will be called, in that case we don't want to do anything.
            if (isConnecting)
            {
                JoinOrCreateRoom();
                isConnecting = false;
            }
        }

        public override void OnLeftRoom()
        {
            Debug.Log("The local client has left the room");
            PhotonNetwork.OfflineMode = false;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
            isConnecting = false;
            creatingRoom = false;
            roomCode = string.Empty;
            PhotonNetwork.OfflineMode = false;
            Debug.LogFormat("Disconnected from room with reason: {0}", cause);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("Created room");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room");

            // We only load if we are the first player, else we rely on
            // PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Loading room");

                // Load the level
                PhotonNetwork.LoadLevel(SceneManagerHelper.ActiveSceneBuildIndex + 1);
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarningFormat("Failed to join room with reason: {0}", message);
            multiplayerMenu.SetActive(true);
            launcherMenu.SetActive(false);
            connectingPanel.SetActive(false);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarningFormat("Failed to create room with reason: {0}", message);
            multiplayerMenu.SetActive(true);
            launcherMenu.SetActive(false);
            connectingPanel.SetActive(false);
        }

        #endregion
    }
}

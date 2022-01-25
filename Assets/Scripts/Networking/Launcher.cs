using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using EasyMeshVR.Core;

namespace EasyMeshVR.Multiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        [SerializeField]
        private GameObject activePanel;

        [SerializeField]
        private GameObject launcherMenu;

        [SerializeField]
        private GameObject multiplayerMenu;

        [SerializeField]
        private RoomListMenu roomListMenu;

        [SerializeField]
        private GameObject settingsMenu;

        [SerializeField]
        private GameObject connectingPanel;

        [SerializeField]
        private GameObject connectingToServerPanel;

        [SerializeField]
        private TMP_InputField createRoomInputField;

        [SerializeField]
        private TMP_InputField joinRoomInputField;

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

        /// <summary>
        /// Keeps track of whether the user is in the process of joining a single player room.
        /// </summary>
        bool joiningSinglePlayer;

        bool isConnectingForFirstTime;

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
            launcherMenu.SetActive(false);
            multiplayerMenu.SetActive(false);
            roomListMenu.gameObject.SetActive(false);
            settingsMenu.SetActive(false);
            connectingPanel.SetActive(false);
            connectingToServerPanel.SetActive(false);
            activePanel.SetActive(true);
        }

        #endregion

        #region Public Methods

        public void OnClickedSinglePlayer()
        {
            joiningSinglePlayer = true;

            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Player previously made a connection to the multiplayer server, disconnecting now...");
                PhotonNetwork.Disconnect();
            }
            else
            {
                CreateSinglePlayerRoom();
            }
        }

        public void OnClickedMultiPlayer()
        {
            // Set offline mode to false just in case it was set to true before
            joiningSinglePlayer = false;
            PhotonNetwork.OfflineMode = false;

            if (!PhotonNetwork.IsConnected)
            {
                SwapActivePanel(connectingToServerPanel);
                isConnectingForFirstTime = PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                SwapActivePanel(multiplayerMenu);
            }
        }

        public void OnClickedRoomList()
        {
            SwapActivePanel(roomListMenu.gameObject);
        }

        public void OnClickedSettings()
        {
            SwapActivePanel(settingsMenu);
        }

        public void OnClickedBackMultiplayer()
        {
            SwapActivePanel(launcherMenu);
        }

        public void OnClickedBackRoomList()
        {
            SwapActivePanel(multiplayerMenu);
        }

        public void OnClickedBackSettings()
        {
            SwapActivePanel(launcherMenu);
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
            SwapActivePanel(connectingPanel);
            this.roomCode = roomCode;

            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsConnectedAndReady)
                {
                    JoinOrCreateRoom();
                }
                else
                {
                    isConnecting = true;
                }
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
                    MaxPlayers = Constants.MAX_PLAYERS_PER_ROOM
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
            joiningSinglePlayer = false;

            // Here we first turn PhotonNetwork.OfflineMode = true and then create
            // the offline room using Photon
            PhotonNetwork.OfflineMode = true;

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

            if (!PhotonNetwork.OfflineMode && !PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("Client joined lobby");

            if (isConnectingForFirstTime)
            {
                isConnectingForFirstTime = false;
                SwapActivePanel(multiplayerMenu);
            }
            // We don't want to do anything if we are not attempting to join/create a room.
            // The case where isConnecting is false is typically when you lost or quit the game,
            // when this level is loaded, OnConnectedToMaster will be called, in that case we don't want to do anything.
            else if (isConnecting)
            {
                JoinOrCreateRoom();
                isConnecting = false;
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            roomListMenu.UpdateRoomlist(roomList);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (joiningSinglePlayer)
            {
                Debug.Log("Disconnected client from master server, now creating singleplayer room...");
                CreateSinglePlayerRoom();
            }
            else
            {
                SwapActivePanel(launcherMenu);
                isConnecting = false;
                creatingRoom = false;
                roomCode = string.Empty;
                PhotonNetwork.OfflineMode = false;
                Debug.LogFormat("Disconnected from server with reason: {0}", cause);
            }
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
            SwapActivePanel(multiplayerMenu);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarningFormat("Failed to create room with reason: {0}", message);
            SwapActivePanel(multiplayerMenu);
        }

        #endregion

        #region Private Methods

        private void SwapActivePanel(GameObject targetPanel)
        {
            activePanel.SetActive(false);
            targetPanel.SetActive(true);
            activePanel = targetPanel;
        }

        #endregion
    }
}

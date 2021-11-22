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
        private TMP_InputField createRoomInputField;

        [SerializeField]
        private TMP_InputField joinRoomInputField;

        /// <summary>
        /// This client's game version number. Users can be separated from each other
        /// by gameVersion which allows you to make breaking changes.
        /// </summary>
        string gameVersion = "1.0.0";

        /// <summary>
        /// Keeps track of the current connection process. Since conneciton is asynchronous
        /// and is based on several callbacks from Photon, we need to keep track of this to
        /// properly adjust the behavior when we callback from Photon.
        /// </summary>
        bool isConnecting;

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
        }

        #endregion

        #region Public Methods

        public void OnClickedSinglePlayer()
        {
            // TODO: figure out how to toggle offline mode back to false if the user quits the singleplayer game

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

        public void OnClickedBackMultiplayer()
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
        }

        public void OnClickedQuit()
        {
            Application.Quit();
        }

        public void OnClickedCreateRoom()
        {
            Debug.Log("Clicked create room button");
        }

        public void OnClickedJoinRoom()
        {
            Debug.Log("Clicked join room button");
        }

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        public void Connect(string roomCode)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRoom(roomCode);
            }
            else
            {
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                this.roomCode = roomCode;
                PhotonNetwork.GameVersion = gameVersion;
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
            
            if (isConnecting)
            {
                PhotonNetwork.JoinRoom(roomCode);
                isConnecting = false;
                roomCode = string.Empty;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
            isConnecting = false;
            roomCode = string.Empty;
            Debug.LogFormat("Disconnected from room with reason: {0}", cause);
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
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarningFormat("Failed to create room with reason: {0}", message);
        }

        #endregion
    }
}

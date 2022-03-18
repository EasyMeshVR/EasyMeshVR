using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using EasyMeshVR.Core;
using EasyMeshVR.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace EasyMeshVR.Multiplayer
{
    public class NetworkPlayerManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        public static NetworkPlayerManager instance { get; private set; }

        #endregion

        #region Private Fields

        [SerializeField]
        private Transform[] spawnPoints = new Transform[Constants.MAX_PLAYERS_PER_ROOM];

        [SerializeField]
        private GameObject networkPlayerPrefab;

        [SerializeField]
        private GameObject XROrigin;

        [SerializeField]
        private HandPresence leftHandRadiusPresence;

        [SerializeField]
        private HandPresence leftHandRayCastPresence;

        [SerializeField]
        private Recorder micRecorder;

        private GameMenuManager radiusGameMenuManager;

        private GameMenuManager raycastGameMenuManager;

        private GameObject spawnedPlayerPrefab;

        private int myPlayerNumber = 0;

        private const string PLAYER_NUMBER_PROPERTY = "playerNumber";

        private Dictionary<int, NetworkPlayer> networkPlayers;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
        }

        void OnValidate()
        {
            if (spawnPoints.Length != Constants.MAX_PLAYERS_PER_ROOM)
            {
                Debug.LogWarning("The number of spawn points should be equal to " + Constants.MAX_PLAYERS_PER_ROOM);
                Array.Resize(ref spawnPoints, Constants.MAX_PLAYERS_PER_ROOM);
            }
        }

        void Start()
        {
            networkPlayers = new Dictionary<int, NetworkPlayer>();
            spawnedPlayerPrefab = SpawnPlayer();
            StartCoroutine(InitializeGameMenuPlayerEntries());
        }

        #endregion

        #region Pun Callbacks

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            if (spawnedPlayerPrefab != null)
            {
                Debug.Log("Destroying player prefab");
                PhotonNetwork.Destroy(spawnedPlayerPrefab);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            CreatePlayerEntry(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            RemovePlayerEntry(otherPlayer);
            RemoveNetworkPlayer(otherPlayer.ActorNumber);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            // Update the host entry
            radiusGameMenuManager.gameMenu.generalOptionsMenuPanel.UpdateHostEntry(newMasterClient);
            raycastGameMenuManager.gameMenu.generalOptionsMenuPanel.UpdateHostEntry(newMasterClient);

            // If the host switched to us, update every other players' entry
            if (newMasterClient == PhotonNetwork.LocalPlayer)
            {
                foreach (Player player in PhotonNetwork.PlayerListOthers)
                {
                    radiusGameMenuManager.gameMenu.generalOptionsMenuPanel.UpdateHostEntry(player);
                    raycastGameMenuManager.gameMenu.generalOptionsMenuPanel.UpdateHostEntry(player);
                }
            }
        }

        #endregion

        #region Public Methods

        public void AddNetworkPlayer(NetworkPlayer networkPlayer)
        {
            networkPlayers.Add(networkPlayer.photonView.OwnerActorNr, networkPlayer);
        }

        public void RemoveNetworkPlayer(int actorNumber)
        {
            networkPlayers.Remove(actorNumber);
        }

        public GameObject SpawnPlayer()
        {
            Transform spawnPoint = GetNextSpawnPoint();
            XROrigin.transform.position = spawnPoint.position;
            XROrigin.transform.rotation = spawnPoint.rotation;

            return PhotonNetwork.Instantiate(networkPlayerPrefab.name, Vector3.zero, Quaternion.identity);
        }

        #endregion

        #region Private Methods

        private IEnumerator InitializeGameMenuPlayerEntries()
        {
            if (!leftHandRadiusPresence.initialized && !leftHandRadiusPresence.initializing)
            {
                leftHandRadiusPresence.TryInitialize();
            }
            if (!leftHandRayCastPresence.initialized && !leftHandRayCastPresence.initializing)
            {
                leftHandRayCastPresence.TryInitialize();
            }

            while (leftHandRadiusPresence.initializing || leftHandRayCastPresence.initializing) 
            {
                Debug.Log("waiting on initialization of left hand presence");
                yield return null;
            }

            raycastGameMenuManager = leftHandRayCastPresence.spawnedHandModel.GetComponent<GameMenuManager>();
            radiusGameMenuManager = leftHandRadiusPresence.spawnedHandModel.GetComponent<GameMenuManager>();

            InitializePlayerList();
        }

        private void InitializePlayerList()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                CreatePlayerEntry(player);
            }
        }

        private void OnKickAction(Player player)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("Kicking player Name: {0} ActorNumber: {1}", player.NickName, player.ActorNumber);
                PhotonNetwork.CloseConnection(player);
            }
            else
            {
                Debug.LogWarningFormat("Cannot kick player Name: {0}. We are not the master client.", player.NickName);
            }
        }

        private void OnMuteAction(Player player)
        {
            Debug.LogFormat("NetworkPlayerManager:OnMuteAction(): Muting player Name {0} ActorNumber {1}", player.NickName, player.ActorNumber);

            // Mute our own mic from transmitting our voice
            if (player == PhotonNetwork.LocalPlayer)
            {
                if (micRecorder.IsRecording)
                {
                    micRecorder.StopRecording();
                }
                else
                {
                    micRecorder.StartRecording();
                }
            }
            // Mute the audio (locally) coming from other players
            else
            {
                NetworkPlayer networkPlayer;
                
                if (networkPlayers.TryGetValue(player.ActorNumber, out networkPlayer) && networkPlayer)
                {
                    networkPlayer.ToggleMuteMic();
                }
                else
                {
                    Debug.LogWarningFormat("Could not mute player Name: {0} ActorNumber: {1}", player.NickName, player.ActorNumber);
                }
            }
        }

        private void CreatePlayerEntry(Player player)
        {
            UnityAction kickAction = delegate { OnKickAction(player); };
            UnityAction muteAction = delegate { OnMuteAction(player); };

            radiusGameMenuManager.gameMenu.generalOptionsMenuPanel.CreatePlayerEntry(player, kickAction, muteAction);
            raycastGameMenuManager.gameMenu.generalOptionsMenuPanel.CreatePlayerEntry(player, kickAction, muteAction);
        }

        private void RemovePlayerEntry(Player player)
        {
            radiusGameMenuManager.gameMenu.generalOptionsMenuPanel.RemovePlayerEntry(player);
            raycastGameMenuManager.gameMenu.generalOptionsMenuPanel.RemovePlayerEntry(player);
        }

        private Transform GetNextSpawnPoint()
        {
            myPlayerNumber = 0;

            Player[] playerList = PhotonNetwork.PlayerList;
            Array.Sort(playerList, (p1, p2) =>
            {
                if (p1 == PhotonNetwork.LocalPlayer)
                {
                    return 1;
                }
                if (p2 == PhotonNetwork.LocalPlayer)
                {
                    return -1;
                }

                return (int)p1.CustomProperties[PLAYER_NUMBER_PROPERTY] - (int)p2.CustomProperties[PLAYER_NUMBER_PROPERTY];
            });

            foreach (Player p in playerList)
            {
                if (p == PhotonNetwork.LocalPlayer) continue;

                Hashtable customProperties = p.CustomProperties;

                int otherPlayerNumber = (int)customProperties[PLAYER_NUMBER_PROPERTY];

                if (myPlayerNumber < otherPlayerNumber || myPlayerNumber > otherPlayerNumber)
                {
                    break;
                }

                myPlayerNumber++;
            }

            Player myPlayer = PhotonNetwork.LocalPlayer;
            Hashtable myProperties = new Hashtable();
            myProperties.Add(PLAYER_NUMBER_PROPERTY, myPlayerNumber);
            myPlayer.SetCustomProperties(myProperties);

            Debug.LogFormat("Assigned playerNumber {0} to local player", myPlayerNumber);

            if (myPlayerNumber < 0 || myPlayerNumber >= Constants.MAX_PLAYERS_PER_ROOM)
            {
                Debug.LogError("Failed to find a valid spawn point for player");
                return null;
            }

            return spawnPoints[myPlayerNumber];
        }

        #endregion
    }
}

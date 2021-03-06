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

        public GameMenuManager radiusGameMenuManager;

        public GameMenuManager raycastGameMenuManager;

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
            else
            {
                Debug.LogError("Failed to destroy spawnedPlayerPrefab because it is null.");
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            CreatePlayerEntry(newPlayer, false);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            RemovePlayerEntry(otherPlayer);
            RemoveNetworkPlayer(otherPlayer.ActorNumber);

            NetworkMeshManager.instance.ClearHeldDataForPlayer(otherPlayer);
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

        public void SetPlayerNamesVisible(bool visible)
        {
            foreach (NetworkPlayer netPlayer in networkPlayers.Values)
            {
                // Don't do anything for my own player name
                if (netPlayer.photonView.IsMine) continue;

                netPlayer.SetPlayerNameVisible(visible);
            }
        }

        public void MuteMicLocal()
        {
            micRecorder.TransmitEnabled = false;
        }

        public void UpdateMuteIcon(bool muted)
        {
            radiusGameMenuManager.gameMenu
                   .generalOptionsMenuPanel.UpdateMuteIcon(PhotonNetwork.LocalPlayer, muted);
            raycastGameMenuManager.gameMenu
                .generalOptionsMenuPanel.UpdateMuteIcon(PhotonNetwork.LocalPlayer, muted);
        }

        public void HideClosePlayers(bool hide)
        {
            foreach (NetworkPlayer netPlayer in networkPlayers.Values)
            {
                if (!netPlayer.photonView.IsMine)
                {
                    netPlayer.HidePlayerAvatar(hide);
                }
            }
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
                yield return null;
            }

            raycastGameMenuManager = leftHandRayCastPresence.spawnedHandModel.GetComponent<GameMenuManager>();
            radiusGameMenuManager = leftHandRadiusPresence.spawnedHandModel.GetComponent<GameMenuManager>();

            InitializePlayerList();
        }

        private void InitializePlayerList()
        {
            NetworkPlayer localNetPlayer = spawnedPlayerPrefab.GetComponent<NetworkPlayer>();

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer && localNetPlayer.photonView.IsMine)
                {
                    bool muteMic = PlayerPrefs.GetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY) != 0;

                    CreatePlayerEntry(player, muteMic);

                    if (muteMic)
                    {
                        MuteMicLocal();
                    }
                }
                else
                {
                    CreatePlayerEntry(player, false);
                }
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
            // Mute our own mic from transmitting our voice
            if (player == PhotonNetwork.LocalPlayer)
            {
                micRecorder.TransmitEnabled = !micRecorder.TransmitEnabled;
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

        private void CreatePlayerEntry(Player player, bool muted)
        {
            UnityAction kickAction = delegate { OnKickAction(player); };
            UnityAction muteAction = delegate { OnMuteAction(player); };

            radiusGameMenuManager.gameMenu.generalOptionsMenuPanel.CreatePlayerEntry(player, kickAction, muteAction, muted);
            raycastGameMenuManager.gameMenu.generalOptionsMenuPanel.CreatePlayerEntry(player, kickAction, muteAction, muted);
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

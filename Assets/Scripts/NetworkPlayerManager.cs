using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using EasyMeshVR.Core;

namespace EasyMeshVR.Multiplayer
{
    public class NetworkPlayerManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        public static NetworkPlayerManager instance;

        #endregion

        #region Private Fields

        [SerializeField]
        private Transform[] spawnPoints = new Transform[Constants.MAX_PLAYERS_PER_ROOM];

        [SerializeField]
        private GameObject XROrigin;

        private GameObject spawnedPlayerPrefab;

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
            spawnedPlayerPrefab = SpawnPlayer();
        }

        #endregion

        #region Pun Callbacks

        public override void OnCreatedRoom()
        {
            Debug.Log("Created room");
            base.OnCreatedRoom();

        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Player joined room");
            base.OnJoinedRoom();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            if (spawnedPlayerPrefab != null)
            {
                PhotonNetwork.Destroy(spawnedPlayerPrefab);
            }
        }

        #endregion

        #region Public Methods

        public static NetworkPlayerManager GetInstance()
        {
            return instance;
        }

        public GameObject SpawnPlayer()
        {
            Transform spawnPoint = GetNextSpawnPoint();
            XROrigin.transform.position = spawnPoint.position;
            XROrigin.transform.rotation = spawnPoint.rotation;

            return PhotonNetwork.Instantiate(Constants.NETWORK_PLAYER_PREFAB_NAME, Vector3.zero, Quaternion.identity);
        }

        #endregion

        #region Private Methods

        private Transform GetNextSpawnPoint()
        {
            Room currentRoom = PhotonNetwork.CurrentRoom;
            int spawnPointIndex = currentRoom.PlayerCount - 1;

            if (spawnPointIndex < 0 || spawnPointIndex >= Constants.MAX_PLAYERS_PER_ROOM)
            {
                Debug.LogError("Failed to spawn player at a valid spawn point");
                return null;
            }

            return spawnPoints[currentRoom.PlayerCount - 1];
        }

        #endregion
    }
}

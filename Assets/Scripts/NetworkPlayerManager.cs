using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerManager : MonoBehaviourPunCallbacks
{
    #region Private Fields

    private GameObject spawnedPlayerPrefab;

    #endregion

    #region Pun Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log("Player joined room");
        base.OnJoinedRoom();
        
        // TODO: add spawn points
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Player", transform.position, transform.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }

    #endregion
}

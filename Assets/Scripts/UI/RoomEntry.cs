using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace EasyMeshVR.UI
{
    public class RoomEntry : MonoBehaviour
    {
        #region Public Fields

        public string roomName 
        {
            get
            {
                return tmpRoomName.text;
            }
            set
            {
                tmpRoomName.text = value;
            }
        }

        // TODO: create separate TMP_Text fields for maxPlayer and playerCount fields
        public int playerCount
        {
            get
            {
                return _playerCount;
            }
            set
            {
                _playerCount = value;
                tmpPlayerCount.text = "Players: " + value + "/" + maxPlayers;
            }
        }

        public int maxPlayers;

        #endregion

        #region Private Fields

        [SerializeField]
        private TMP_Text tmpRoomName;

        [SerializeField]
        private TMP_Text tmpPlayerCount;

        private int _playerCount;

        #endregion
    }
}

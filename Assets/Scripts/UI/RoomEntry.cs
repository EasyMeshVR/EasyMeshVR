using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        public int playerCount
        {
            get
            {
                return _playerCount;
            }
            set
            {
                _playerCount = value;
                UpdatePlayerCountText();
            }
        }

        public int maxPlayers
        {
            get
            {
                return _maxPlayers;
            }
            set
            {
                _maxPlayers = value;
                UpdatePlayerCountText();
            }
        }

        #endregion

        #region Private Fields

        [SerializeField]
        private TMP_Text tmpRoomName;

        [SerializeField]
        private TMP_Text tmpPlayerCount;

        [SerializeField]
        private TMP_Text joinButtonText;

        [SerializeField]
        private Button joinButton;

        [SerializeField]
        private Color openRoomTextColor;

        [SerializeField]
        private Color fullRoomTextColor;

        private int _playerCount;
        private int _maxPlayers;

        const string JOIN_BTN_TEXT = "JOIN";
        const string FULL_ROOM_JOIN_BTN_TEXT = "FULL";

        #endregion

        #region Public Methods

        public void AddJoinButtonOnClickAction(UnityEngine.Events.UnityAction onClickAction)
        {
            joinButton.onClick.AddListener(onClickAction);
        }

        #endregion

        #region Private Methods

        private void UpdatePlayerCountText()
        {
            tmpPlayerCount.text = "Players: " + _playerCount + "/" + _maxPlayers;

            if (_playerCount >= _maxPlayers)
            {
                tmpPlayerCount.color = fullRoomTextColor;
                joinButtonText.text = FULL_ROOM_JOIN_BTN_TEXT;
                joinButton.enabled = false;
            }
            else
            {
                tmpPlayerCount.color = openRoomTextColor;
                joinButtonText.text = JOIN_BTN_TEXT;
                joinButton.enabled = true;
            }
        }

        #endregion
    }
}

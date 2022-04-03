using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

namespace EasyMeshVR.UI
{
    public class PlayerEntry : MonoBehaviour
    {
        #region Public Fields

        public string playerName 
        { 
            get
            {
                return tmpPlayerName.text;
            }
            set
            {
                tmpPlayerName.text = value;
            }
        }

        public bool isHost
        {
            get
            {
                return _isHost;
            }

            set
            {
                _isHost = value;
                SetHostCrownVisible();
                SetKickButtonEnabled();
            }
        }

        public bool isMuted
        {
            get
            {
                return _isMuted;
            }

            set
            {
                _isMuted = value;
                SetMuteButtonImage();
            }
        }

        public Player player { get; set; }

        #endregion

        #region Private Fields

        [SerializeField]
        private TMP_Text tmpPlayerName;

        [SerializeField]
        private Button kickButton;

        [SerializeField]
        private Image kickButtonIcon;

        [SerializeField]
        private Button muteButton;

        [SerializeField]
        private Image muteButtonIcon;

        [SerializeField]
        private Sprite unmutedSprite;

        [SerializeField]
        private Sprite mutedSprite;

        [SerializeField]
        private Image hostCrownIcon;

        private bool _isHost = false;

        private bool _isMuted = false;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            SetHostCrownVisible();
            SetKickButtonEnabled();
        }

        private void Update()
        {
            if (playerName != player.NickName)
            {
                playerName = player.NickName;
            }
        }

        #endregion

        #region Public Methods

        public void AddKickButtonOnClickAction(UnityEngine.Events.UnityAction onClickAction)
        {
            kickButton.onClick.AddListener(onClickAction);
        }

        public void AddMuteButtonOnClickAction(UnityEngine.Events.UnityAction onClickAction)
        {
            muteButton.onClick.AddListener(onClickAction);
        }

        public void ToggleMuteIconSprite()
        {
            _isMuted = !_isMuted;
            SetMuteButtonImage();
        }

        #endregion

        #region Private Methods

        private void SetHostCrownVisible()
        {
            hostCrownIcon.enabled = _isHost;
        }

        private void SetKickButtonEnabled()
        {
            bool enabled = !_isHost && PhotonNetwork.IsMasterClient;
            kickButtonIcon.enabled = enabled;
            kickButton.enabled = enabled;
        }

        private void SetMuteButtonImage()
        {
            if (_isMuted)
            {
                muteButtonIcon.sprite = mutedSprite;
            }
            else
            {
                muteButtonIcon.sprite = unmutedSprite;
            }
        }

        #endregion
    }
}

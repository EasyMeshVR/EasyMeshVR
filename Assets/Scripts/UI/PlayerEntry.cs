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
        private Image hostCrownIcon;

        private bool _isHost = false;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            SetHostCrownVisible();
            SetKickButtonEnabled();
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

        #endregion
    }
}

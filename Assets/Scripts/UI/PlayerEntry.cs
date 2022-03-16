using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
                // TODO: toggle crown image
            }
        }

        #endregion

        #region Private Fields

        [SerializeField]
        private TMP_Text tmpPlayerName;

        [SerializeField]
        private Button kickButton;

        [SerializeField]
        private Button muteButton;

        [SerializeField]
        private Image hostCrownIcon;

        private bool _isHost;

        #endregion
    }
}

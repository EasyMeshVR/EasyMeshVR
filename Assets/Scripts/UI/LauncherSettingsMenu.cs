using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EasyMeshVR.Core;
using Photon.Pun;

namespace EasyMeshVR.UI
{
    public class LauncherSettingsMenu : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private TMP_InputField playerNameInputField;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            playerNameInputField.text = PlayerPrefs.GetString(Constants.PLAYER_NAME_PREF_KEY, Constants.PLAYER_NAME_PREF_DEFAULT);
        }

        #endregion

        #region Public Methods

        public void UpdatePlayerNamePref(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, Constants.PLAYER_NAME_PREF_DEFAULT);
                return;
            }

            PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, playerName);
            PhotonNetwork.LocalPlayer.NickName = playerName;
        }

        #endregion
    }
}

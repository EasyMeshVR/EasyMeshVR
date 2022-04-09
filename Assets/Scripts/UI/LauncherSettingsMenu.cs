using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EasyMeshVR.Core;
using Photon.Pun;
using UnityEngine.UI;

namespace EasyMeshVR.UI
{
    public class LauncherSettingsMenu : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private TMP_InputField playerNameInputField;

        [SerializeField]
        private Toggle hideClosePlayersToggle;

        [SerializeField]
        private Toggle hidePlayerNamesToggle;

        [SerializeField]
        private Toggle muteMicOnJoinToggle;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            playerNameInputField.text = PlayerPrefs.GetString(Constants.PLAYER_NAME_PREF_KEY, Constants.PLAYER_NAME_PREF_DEFAULT);
            hideClosePlayersToggle.isOn = IntToBool(PlayerPrefs.GetInt(Constants.HIDE_CLOSE_PLAYERS_PREF_KEY, Constants.HIDE_CLOSE_PLAYERS_PREF_DEFAULT));
            hidePlayerNamesToggle.isOn = IntToBool(PlayerPrefs.GetInt(Constants.HIDE_PLAYER_NAMES_PREF_KEY, Constants.HIDE_PLAYER_NAMES_PREF_DEFAULT));
            muteMicOnJoinToggle.isOn = IntToBool(PlayerPrefs.GetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY, Constants.MUTE_MIC_ON_JOIN_PREF_DEFAULT));
        }

        #endregion

        #region Public Methods

        public static bool IntToBool(int intVal)
        {
            return (intVal != 0);
        }

        public void UpdatePlayerNamePref(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, Constants.PLAYER_NAME_PREF_DEFAULT);
                return;
            }

            PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, playerName);
            PhotonNetwork.LocalPlayer.NickName = playerName;
        }

        public void UpdateHideClosePlayersPref(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.HIDE_CLOSE_PLAYERS_PREF_KEY, toggled ? 1 : 0);
        }

        public void UpdateHidePlayerNamesPref(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.HIDE_PLAYER_NAMES_PREF_KEY, toggled ? 1 : 0);
        }

        public void UpdateMuteMicOnJoin(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY, toggled ? 1 : 0);
        }

        #endregion
    }
}

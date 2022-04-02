using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMeshVR.Core;
using EasyMeshVR.Multiplayer;
using Photon.Pun;

namespace EasyMeshVR.UI
{
    public class GameSettingsMenu : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Toggle hidePlayerNamesToggle;
        [SerializeField] private Toggle muteMicOnJoinToggle;
        [SerializeField] private Toggle hideClosePlayersToggle;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            bool playerNamesHidden = ArePlayerNamesHidden();
            bool isMicMutedOnJoin = IsMicMutedOnJoin();
            bool hideClosePlayers = AreClosePlayersHidden();

            hidePlayerNamesToggle.isOn = playerNamesHidden;
            muteMicOnJoinToggle.isOn = isMicMutedOnJoin;
            hideClosePlayersToggle.isOn = hideClosePlayers;
        }

        #endregion

        #region Public Methods

        public void OnClickedChangeDisplayNameButton()
        {
            KeyInputManager.instance.EnableKeyboardForChangingDisplayName(OnClickedChangeDisplayName);
        }

        public void OnClickedChangeDisplayName(string displayName)
        {
            PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, displayName);
            PhotonNetwork.LocalPlayer.NickName = displayName;
            KeyInputManager.instance.DisplaySuccessMessage("Changed display name.");
        }

        public void OnToggledHidePlayerNames(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.HIDE_PLAYER_NAMES_PREF_KEY, toggled ? 1 : 0);
            NetworkPlayerManager.instance.SetPlayerNamesVisible(!toggled);
        }

        public void OnToggledMuteMicOnJoin(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY, toggled ? 1 : 0);
        }

        public void OnToggledHideClosePlayers(bool toggled)
        {
            PlayerPrefs.SetInt(Constants.HIDE_CLOSE_PLAYERS_PREF_KEY, toggled ? 1 : 0);

            // TODO: enable sphere collider on local player
        }

        #endregion

        #region Private Methods

        private bool ArePlayerNamesHidden()
        {
            return PlayerPrefs.GetInt(Constants.HIDE_PLAYER_NAMES_PREF_KEY) != 0;
        }

        private bool IsMicMutedOnJoin()
        {
            return PlayerPrefs.GetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY) != 0;
        }

        private bool AreClosePlayersHidden()
        {
            return PlayerPrefs.GetInt(Constants.HIDE_CLOSE_PLAYERS_PREF_KEY) != 0;
        }

        #endregion
    }
}

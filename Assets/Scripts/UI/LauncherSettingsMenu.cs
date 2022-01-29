using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EasyMeshVR.Core;

namespace EasyMeshVR.UI
{
    public class LauncherSettingsMenu : MonoBehaviour
    {
        #region Private Fields

        #endregion

        #region Public Methods

        public void UpdatePlayerNamePref(string playerName)
        {
            PlayerPrefs.SetString(Constants.PLAYER_NAME_PREF_KEY, playerName);
            Debug.Log(PlayerPrefs.GetString(Constants.PLAYER_NAME_PREF_KEY, Constants.PLAYER_NAME_PREF_DEFAULT));
        }

        #endregion
    }
}

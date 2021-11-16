using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace EasyMeshVR.Multiplayer
{
    public class Launcher : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private GameObject launcherMenu;
        [SerializeField]
        private GameObject multiplayerMenu;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
        }

        #endregion

        #region Public Methods

        public void OnClickedSinglePlayer()
        {
            // Here we should first turn PhotonNetwork.OfflineMode = true and then "connect"
            // to the offline room using photon
            Debug.Log("Clicked Single Player");
        }

        public void OnClickedMultiPlayer()
        {
            launcherMenu.SetActive(false);
            multiplayerMenu.SetActive(true);
        }

        public void OnClickedBackMultiplayer()
        {
            launcherMenu.SetActive(true);
            multiplayerMenu.SetActive(false);
        }

        public void OnClickedQuit()
        {
            Application.Quit();
        }

        #endregion
    }
}

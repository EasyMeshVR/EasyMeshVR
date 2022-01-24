using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

namespace EasyMeshVR.UI
{
    public class MainMenu : MonoBehaviour
    {
        #region Private Fields

        #endregion

        #region Public Methods

        public void OnClickedQuitButton()
        {
            // TODO: FIX quit to main menu button doesnt work when leaving as a client from a multiplayer room
            gameObject.SetActive(false);
            PhotonNetwork.LeaveRoom();
        }

        public void OnClickedCancelQuitButton()
        {
            //gameMenu.SwapActivePanels(toolsPanel);
        }

        public void OnClickedUploadButton()
        {
            Debug.Log("clicked upload button!");
        }

        #endregion
    }
}

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
            //PhotonNetwork.LeaveRoom(false);
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

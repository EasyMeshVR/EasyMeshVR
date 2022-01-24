using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace EasyMeshVR.UI
{
    public class GameMenu : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        [SerializeReference]
        GameObject activeMenuPanel;

        [SerializeReference]
        GameObject toolsPanel;

        [SerializeReference]
        MainMenu mainMenuPanel;

        [SerializeReference]
        GameObject settingsPanel;

        #endregion

        #region Public Methods

        /*
        * Left side panel buttons
        */
        public void OnClickedToolsButton()
        {
            SwapActivePanels(toolsPanel);
            Debug.Log("clicked tools");
        }

        public void OnClickedMainMenuButton()
        {
            SwapActivePanels(mainMenuPanel.gameObject);
            Debug.Log("clicked exit");
        }

        public void OnClickedSettingsButton()
        {
            SwapActivePanels(settingsPanel);
            Debug.Log("clicked settings");
        }

        #endregion

        #region Private Methods

        private void SwapActivePanels(GameObject targetPanel)
        {
            activeMenuPanel.SetActive(false);
            targetPanel.SetActive(true);
            activeMenuPanel = targetPanel;
        }

        public override void OnLeftRoom()
        {
            Debug.Log("The local player has left the room");
            PhotonNetwork.OfflineMode = false;
            StartCoroutine(AsyncLoadLauncherScene());
        }

        private IEnumerator AsyncLoadLauncherScene()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        #endregion
    }
}

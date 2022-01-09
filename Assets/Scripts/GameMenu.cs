using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace EasyMeshVR.UI
{
    public class GameMenu : MonoBehaviourPunCallbacks
    {
        [SerializeReference]
        GameObject activeMenuPanel;

        [SerializeReference]
        GameObject toolsPanel;

        [SerializeReference]
        GameObject savePanel;

        [SerializeReference]
        GameObject settingsPanel;

        [SerializeReference]
        GameObject quitPanel;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /*
        * Left side panel buttons
        */
        public void OnClickedToolsButton()
        {
            SwapActivePanels(toolsPanel);
            Debug.Log("clicked tools");
        }

        public void OnClickedSaveButton()
        {
            SwapActivePanels(savePanel);
            Debug.Log("clicked save");
        }

        public void OnClickedSettingsButton()
        {
            SwapActivePanels(settingsPanel);
            Debug.Log("clicked settings");
        }

        public void OnClickedExitButton()
        {
            SwapActivePanels(quitPanel);
            Debug.Log("clicked exit");
        }

        /*
         * Right side main panel buttons
         */
        public void OnClickedQuitButton()
        {
            // TODO: FIX quit to main menu button doesnt work when leaving as a client from a multiplayer room
            gameObject.SetActive(false);
            PhotonNetwork.LeaveRoom();
        }

        public void OnClickedCancelQuitButton()
        {
            SwapActivePanels(toolsPanel);
        }

        private void SwapActivePanels(GameObject targetPanel)
        {
            activeMenuPanel.SetActive(false);
            targetPanel.SetActive(true);
            activeMenuPanel = targetPanel;
        }

        public override void OnLeftRoom()
        {
            Debug.Log("loading level launcher");
            PhotonNetwork.LoadLevel(0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace EasyMeshVR.UI
{
    public class GameMenu : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        [SerializeField]
        public GeneralOptionsMenu generalOptionsMenuPanel;

        [SerializeField]
        public AbilitiesMenu abilitiesMenuPanel;

        [SerializeField]
        public GameSettingsMenu settingsMenuPanel;

        #endregion

        #region Private Fields

        [SerializeField]
        GameObject activeMenuPanel;

        [SerializeField]
        GameObject activeMainOption;

        [SerializeField]
        GameObject mainMenuOption;

        [SerializeField]
        GameObject toolsOption;

        [SerializeField]
        GameObject settingsOption;

        [SerializeField]
        Color mainOptionDefaultColor;

        [SerializeField]
        Color mainOptionSelectedColor;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            // Set the colors of the main option buttons
            SetMainOptionColor(mainMenuOption, mainOptionDefaultColor);
            SetMainOptionColor(toolsOption, mainOptionDefaultColor);
            SetMainOptionColor(settingsOption, mainOptionDefaultColor);
            SetMainOptionColor(activeMainOption, mainOptionSelectedColor);

            // Disable all panels except the active one
            abilitiesMenuPanel.gameObject.SetActive(false);
            generalOptionsMenuPanel.gameObject.SetActive(false);
            settingsMenuPanel.gameObject.SetActive(false);
            activeMenuPanel.SetActive(true);
        }

        #endregion

        #region Main Options Bar Button Methods

        public void OnClickedToolsButton()
        {
            SwapActiveMainOption(toolsOption);
            SwapActivePanels(abilitiesMenuPanel.gameObject);
        }

        public void OnClickedGeneralOptionsButton()
        {
            SwapActiveMainOption(mainMenuOption);
            SwapActivePanels(generalOptionsMenuPanel.gameObject);
        }

        public void OnClickedSettingsButton()
        {
            SwapActiveMainOption(settingsOption);
            SwapActivePanels(settingsMenuPanel.gameObject);
        }

        #endregion

        #region Private Methods

        private void SwapActivePanels(GameObject targetPanel)
        {
            activeMenuPanel.SetActive(false);
            targetPanel.SetActive(true);
            activeMenuPanel = targetPanel;
        }

        private void SwapActiveMainOption(GameObject targetMainOption)
        {
            SetMainOptionColor(activeMainOption, mainOptionDefaultColor);
            SetMainOptionColor(targetMainOption, mainOptionSelectedColor);
            activeMainOption = targetMainOption;
        }

        private void SetMainOptionColor(GameObject mainOption, Color color)
        {
            mainOption.GetComponent<Image>().color = color;
        }

        #endregion
    }
}

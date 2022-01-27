using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using EasyMeshVR.Core;

namespace EasyMeshVR.UI
{
    public class MainMenu : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private GameObject activePanel;

        [SerializeField]
        private GameObject activeSubOptionButton;

        [SerializeField]
        private GameObject cloudUploadSubOption;

        [SerializeField]
        private GameObject cloudDownloadSubOption;

        [SerializeField]
        private GameObject newModelSubOption;

        [SerializeField]
        private GameObject localSaveSubOption;

        [SerializeField]
        private GameObject cloudUploadPanel;

        [SerializeField]
        private GameObject cloudDownloadPanel;

        [SerializeField]
        private GameObject newModelPanel;

        [SerializeField]
        private GameObject localSavePanel;

        [SerializeField]
        private TMP_Text exportModelButtonText;

        [SerializeField]
        private Button exportModelButton;

        [SerializeField]
        private Color subOptionDefaultColor;

        [SerializeField]
        private Color subOptionSelectedColor;

        #endregion

        #region MonoBehaviourCallbacks

        void Start()
        {
            // Set colors of sub-option buttons
            SetSubOptionButtonColor(cloudUploadSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(cloudDownloadSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(localSaveSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(newModelSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(activeSubOptionButton, subOptionSelectedColor);

            // Disable all MainMenu panels except the active one
            cloudUploadPanel.SetActive(false);
            cloudDownloadPanel.SetActive(false);
            newModelPanel.SetActive(false);
            localSavePanel.SetActive(false);
            activePanel.SetActive(true);
        }

        #endregion

        #region Sub Options Button Methods

        public void OnClickedCloudUploadSubOption()
        {
            SwapActiveSubOptionButton(cloudUploadSubOption);
            SwapActivePanel(cloudUploadPanel);
        }

        public void OnClickedCloudDownloadSubOption()
        {
            SwapActiveSubOptionButton(cloudDownloadSubOption);
            SwapActivePanel(cloudDownloadPanel);
        }

        public void OnClickedNewModelSubOption()
        {
            SwapActiveSubOptionButton(newModelSubOption);
            SwapActivePanel(newModelPanel);
        }

        public void OnClickedLocalSaveSubOption()
        {
            SwapActiveSubOptionButton(localSaveSubOption);
            SwapActivePanel(localSavePanel);
        }

        #endregion

        #region Cloud Upload Panel Methods

        public void OnClickedExportModel()
        {
            exportModelButtonText.text = "Exporting...";
            exportModelButton.enabled = false;

            Debug.Log("Exporting current model mesh to the cloud...");
            ModelImportExport.instance.ExportModel(true, ModelCodeType.DIGIT, ExportCallback);
        }

        #endregion

        #region Settings Panel Methods

        public void OnClickedQuitButton()
        {
            //PhotonNetwork.LeaveRoom(false);
            PhotonNetwork.LeaveRoom();
        }

        public void OnClickedCancelQuitButton()
        {
            //gameMenu.SwapActivePanels(toolsPanel);
        }

        #endregion

        #region Import/Export Callbacks

        private void ExportCallback(string modelCode, string error)
        {
            exportModelButton.enabled = true;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when uploading model: {0}", error);
                exportModelButtonText.text = error;
                return;
            }

            Debug.LogFormat("Successfully uploaded model, your model code is {0}", modelCode);
            exportModelButtonText.text = modelCode;
        }

        #endregion

        #region Private Methods

        private void SwapActivePanel(GameObject targetPanel)
        {
            activePanel.SetActive(false);
            targetPanel.SetActive(true);
            activePanel = targetPanel;
        }

        private void SwapActiveSubOptionButton(GameObject targetSubOptionButton)
        {
            SetSubOptionButtonColor(activeSubOptionButton, subOptionDefaultColor);
            SetSubOptionButtonColor(targetSubOptionButton, subOptionSelectedColor);
            activeSubOptionButton = targetSubOptionButton;
        }

        private void SetSubOptionButtonColor(GameObject subOptionButton, Color color)
        {
            subOptionButton.GetComponent<Image>().color = color;
        }

        #endregion
    }
}
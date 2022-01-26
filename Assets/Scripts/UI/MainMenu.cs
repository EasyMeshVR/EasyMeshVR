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
        private Color subOptionDefaultColor;

        [SerializeField]
        private Color subOptionSelectedColor;

        #endregion

        #region Sub Options Button Methods

        public void OnClickedCloudUploadSubOption()
        {
            Debug.Log("clicked cloud upload button");
            SwapActiveSubOptionButton(cloudUploadSubOption);
            SwapActivePanel(cloudUploadPanel);
        }

        public void OnClickedCloudDownloadSubOption()
        {
            Debug.Log("clicked cloud download button");
            SwapActiveSubOptionButton(cloudDownloadSubOption);
            SwapActivePanel(cloudDownloadSubOption);
        }

        public void OnClickedNewModelSubOption()
        {
            Debug.Log("clicked new model button");
            SwapActiveSubOptionButton(newModelSubOption);
            SwapActivePanel(newModelPanel);
        }

        public void OnClickedLocalSaveSubOption()
        {
            Debug.Log("clicked local save button");
            SwapActiveSubOptionButton(localSaveSubOption);
            SwapActivePanel(localSavePanel);
        }

        #endregion

        #region Cloud Upload Panel Methods

        public void OnClickedExportModel()
        {
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
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when uploading model: {0}", error);
                return;
            }

            Debug.LogFormat("Successfully uploaded model, your model code is {0}", modelCode);
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
            activeSubOptionButton.GetComponent<Image>().color = subOptionDefaultColor;
            targetSubOptionButton.GetComponent<Image>().color = subOptionSelectedColor;
            activeSubOptionButton = targetSubOptionButton;
        }

        #endregion
    }
}
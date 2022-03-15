using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using EasyMeshVR.Core;
using EasyMeshVR.Multiplayer;

namespace EasyMeshVR.UI
{
    public class GeneralOptionsMenu
        : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private GameObject activePanel;

        [SerializeField]
        private GameObject activeSubOption;

        [SerializeField]
        private GameObject saveQuitSubOption;

        [SerializeField]
        private GameObject cloudUploadDownloadSubOption;

        [SerializeField]
        private GameObject clearCanvasSubOption;

        [SerializeField]
        private GameObject multiplayerSubOption;

        [SerializeField]
        private GameObject saveQuitPanel;

        [SerializeField]
        private GameObject cloudUploadDownloadPanel;

        [SerializeField]
        private GameObject clearCanvasPanel;

        [SerializeField]
        private GameObject multiplayerPanel;

        [SerializeField]
        private TMP_Text exportModelButtonText;

        [SerializeField]
        private Button exportModelButton;

        [SerializeField]
        private TMP_InputField importModelInputField;

        [SerializeField]
        private Color subOptionDefaultColor;

        [SerializeField]
        private Color subOptionSelectedColor;

        #endregion

        #region MonoBehaviourCallbacks

        void Start()
        {
            // Set colors of sub-option buttons
            SetSubOptionButtonColor(saveQuitSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(cloudUploadDownloadSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(clearCanvasSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(multiplayerSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(activeSubOption, subOptionSelectedColor);

            // Disable all MainMenu panels except the active one
            saveQuitPanel.SetActive(false);
            cloudUploadDownloadPanel.SetActive(false);
            clearCanvasPanel.SetActive(false);
            multiplayerPanel.SetActive(false);
            activePanel.SetActive(true);
        }

        #endregion

        #region Sub Options Button Methods

        public void OnClickedSaveQuitSubOption()
        {
            SwapActiveSubOptionButton(saveQuitSubOption);
            SwapActivePanel(saveQuitPanel);
        }

        public void OnClickedCloudUploadDownloadSubOption()
        {
            SwapActiveSubOptionButton(cloudUploadDownloadSubOption);
            SwapActivePanel(cloudUploadDownloadPanel);
        }

        public void OnClickedClearCanvasSubOption()
        {
            SwapActiveSubOptionButton(clearCanvasSubOption);
            SwapActivePanel(clearCanvasPanel);
        }

        public void OnClickedMultiplayerSubOption()
        {
            SwapActiveSubOptionButton(multiplayerSubOption);
            SwapActivePanel(multiplayerPanel);
        }

        #endregion

        #region Cloud Upload Panel Methods

        public void OnClickedImportModel()
        {
            if (string.IsNullOrWhiteSpace(importModelInputField.text))
            {
                Debug.Log("Cannot import a model with empty code!");
                return;
            }
            Debug.Log("clicked import model");
            NetworkMeshManager.instance.SynchronizeMeshImport(importModelInputField.text, ImportCallback);
        }

        public void OnClickedExportModel()
        {
            exportModelButtonText.text = "Exporting...";
            exportModelButton.enabled = false;

            Debug.Log("Exporting current model mesh to the cloud...");
            ModelImportExport.instance.ExportModel(true, ModelCodeType.DIGIT, ExportCallback);
        }

        #endregion

        #region Save Quit Button Methods

        public void OnClickedQuitButton()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void OnClickedCancelQuitButton()
        {
            //gameMenu.SwapActivePanels(toolsPanel);
        }

        #endregion

        #region Import/Export Callbacks

        private void ImportCallback(bool success)
        {
            if (!success)
            {
                Debug.Log("Error encountered while importing mesh!");
                return;
            }

            Debug.Log("GeneralOptionsMenu: Successfully improted model into scene");
        }

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

        #region Clear Canvas Methods

        public void OnClickedClearCanvasButton()
        {
            NetworkMeshManager.instance.SynchronizeClearCanvas();
        }

        #endregion

        #region Private Methods

        private void SwapActivePanel(GameObject targetPanel)
        {
            activePanel.SetActive(false);
            targetPanel.SetActive(true);
            activePanel = targetPanel;
        }

        private void SwapActiveSubOptionButton(GameObject targetSubOption)
        {
            SetSubOptionButtonColor(activeSubOption, subOptionDefaultColor);
            SetSubOptionButtonColor(targetSubOption, subOptionSelectedColor);
            activeSubOption = targetSubOption;
        }

        private void SetSubOptionButtonColor(GameObject subOptionButton, Color color)
        {
            subOptionButton.GetComponent<Image>().color = color;
        }

        #endregion
    }
}
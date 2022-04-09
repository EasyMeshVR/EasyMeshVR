using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using TMPro;
using Photon.Pun;

namespace EasyMeshVR.UI
{
    public class Keyboard : MonoBehaviour
    {
        [SerializeField] XRGrabInteractable grabInteractable;
        [SerializeField] Transform defaultBoardAttachTransform;
        [SerializeField] Transform numpadBoardAttachTransform;
        [SerializeField] Material unselected;
        [SerializeField] Material hovered;
        [SerializeField] Material selected;
        [SerializeField] MeshRenderer defaultBoardMeshRenderer;
        [SerializeField] MeshRenderer numpadBoardMeshRenderer;

        [SerializeField] GameObject defaultBoard;
        [SerializeField] GameObject numpadBoard;

        [SerializeField] TMP_InputField activeInputField;
        [SerializeField] TMP_InputField defaultPanelInputField;
        [SerializeField] TMP_InputField importModelPanelInputField;
        [SerializeField] TMP_InputField changeDisplayNamePanelInputField;

        [SerializeField] ButtonVR activeEnterButton;
        [SerializeField] ButtonVR enterButtonDefault;
        [SerializeField] ButtonVR enterButtonNumpad;

        [SerializeField] GameObject defaultPanel;
        [SerializeField] GameObject importModelPanel;
        [SerializeField] GameObject changeDisplayNamePanel;

        [SerializeField] TMP_Text activeErrorText;
        [SerializeField] TMP_Text importModelErrorText;
        [SerializeField] TMP_Text changeDisplayNameErrorText;

        [SerializeField] TMP_Text activeSuccesText;
        [SerializeField] TMP_Text importModelSuccessText;
        [SerializeField] TMP_Text changeDisplayNameSuccessText;

        [SerializeField] GameObject normalButtons;
        [SerializeField] GameObject capsButtons;

        [SerializeField] bool caps;

        [Serializable]
        public class KeyboardUpdateEvent : UnityEvent<string> { }

        [Serializable]
        public class KeyboardSubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class KeyboardCancelEvent : UnityEvent { }

        public KeyboardUpdateEvent OnUpdate = new KeyboardUpdateEvent();
        public KeyboardSubmitEvent OnSubmit = new KeyboardSubmitEvent();
        public KeyboardCancelEvent OnCancel = new KeyboardCancelEvent();

        private Transform mainCameraTransform;
        private static Regex digitCodeRegex = new Regex(@"^\d{6}$");

        void Awake()
        {
            mainCameraTransform = Camera.main.transform;
            caps = false;
            DisplayDefaultPanel();
        }

        void OnEnable()
        {
            defaultBoardMeshRenderer.material = unselected;
            numpadBoardMeshRenderer.material = unselected;

            // Rotate the keyboard around its y-axis based on the mainCamera's y rotation
            transform.rotation = Quaternion.identity;
            transform.RotateAround(transform.position, Vector3.up, mainCameraTransform.rotation.eulerAngles.y);

            // Posiiton the keyboard in front of the mainCamera
            Vector3 forwardVec = Vector3.Scale(0.35f * Vector3.one, transform.rotation * Vector3.forward);
            transform.position = mainCameraTransform.position + new Vector3(0f, -0.6f, 0f);
            transform.position += forwardVec;

            grabInteractable.hoverEntered.AddListener(HoverOver);
            grabInteractable.hoverExited.AddListener(HoverExit);
            grabInteractable.selectEntered.AddListener(GrabPulled);
            grabInteractable.selectExited.AddListener(GrabReleased);
        }

        void OnDisable()
        {
            grabInteractable.hoverEntered.RemoveListener(HoverOver);
            grabInteractable.hoverExited.RemoveListener(HoverExit);
            grabInteractable.selectEntered.RemoveListener(GrabPulled);
            grabInteractable.selectExited.RemoveListener(GrabReleased);
        }

        MeshRenderer GetActiveBoardMeshRenderer()
        {
            return (activeEnterButton == enterButtonDefault) ? defaultBoardMeshRenderer : numpadBoardMeshRenderer;
        }

        void HoverOver(HoverEnterEventArgs arg0)
        {
            GetActiveBoardMeshRenderer().material = hovered;
        }

        void HoverExit(HoverExitEventArgs arg0)
        {
            GetActiveBoardMeshRenderer().material = unselected;
        }

        void GrabPulled(SelectEnterEventArgs arg0)
        {
            GetActiveBoardMeshRenderer().material = selected;
        }

        void GrabReleased(SelectExitEventArgs arg0)
        {
            GetActiveBoardMeshRenderer().material = unselected;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void DisplayImportModelPanel()
        {
            numpadBoard.SetActive(true);
            defaultBoard.SetActive(false);
            activeInputField = importModelPanelInputField;
            activeEnterButton = enterButtonNumpad;
            activeErrorText = importModelErrorText;
            activeSuccesText = importModelSuccessText;
            defaultPanel.SetActive(false);
            importModelPanel.SetActive(true);
            changeDisplayNamePanel.SetActive(false);
            grabInteractable.attachTransform = numpadBoardAttachTransform;
        }

        public void DisplayDefaultPanel()
        {
            numpadBoard.SetActive(false);
            defaultBoard.SetActive(true);
            activeInputField = defaultPanelInputField;
            activeEnterButton = enterButtonDefault;
            defaultPanel.SetActive(true);
            importModelPanel.SetActive(false);
            changeDisplayNamePanel.SetActive(false);
            grabInteractable.attachTransform = defaultBoardAttachTransform;
        }

        public void DisplayNameChangePanel()
        {
            numpadBoard.SetActive(false);
            defaultBoard.SetActive(true);
            activeInputField = changeDisplayNamePanelInputField;
            changeDisplayNamePanelInputField.text = PhotonNetwork.LocalPlayer.NickName;
            activeEnterButton = enterButtonDefault;
            activeErrorText = changeDisplayNameErrorText;
            activeSuccesText = changeDisplayNameSuccessText;
            defaultPanel.SetActive(false);
            importModelPanel.SetActive(false);
            changeDisplayNamePanel.SetActive(true);
            grabInteractable.attachTransform = defaultBoardAttachTransform;
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            activeInputField.text = text;

            OnUpdate.Invoke(activeInputField.text);
        }

        public void Submit()
        {
            OnSubmit.Invoke(activeInputField.text);
        }

        public void Cancel()
        {
            OnCancel.Invoke();
        }

        public void AddEnterButtonOnReleaseEvent(UnityAction onReleaseAction)
        {
            activeEnterButton.onRelease.AddListener(onReleaseAction);
        }

        public void AddImportEnterButtonOnReleaseEvent(Action<string> onReleaseAction)
        {
            activeEnterButton.onRelease.AddListener(delegate
            {
                HandleImportModel(onReleaseAction);
            });
        }
        
        public void AddChangeDisplayNameEnterButtonOnReleaseEvent(Action<string> onReleaseAction)
        {
            activeEnterButton.onRelease.AddListener(delegate
            {
                HandleChangeDisplayName(onReleaseAction);
            });
        }

        private void HandleImportModel(Action<string> onReleaseAction)
        {
            ClearErrorMessage();
            ClearSuccessMessage();

            if (!digitCodeRegex.IsMatch(activeInputField.text))
            {
                DisplayErrorMessage("Please enter a 6-digit code.");
                return;
            }

            onReleaseAction.Invoke(activeInputField.text);
        }

        private void HandleChangeDisplayName(Action<string> onReleaseAction)
        {
            ClearErrorMessage();
            ClearSuccessMessage();

            if (string.IsNullOrWhiteSpace(activeInputField.text))
            {
                DisplayErrorMessage("Name must be non-empty");
                return;
            }

            onReleaseAction.Invoke(activeInputField.text);
        }

        public void DisplayErrorMessage(string errorMsg)
        {
            activeErrorText.text = errorMsg;
            activeErrorText.gameObject.SetActive(true);
        }

        public void DisplaySuccessMessage(string successMsg)
        {
            activeSuccesText.text = successMsg;
            activeSuccesText.gameObject.SetActive(true);
        }

        public void ClearErrorMessage()
        {
            activeErrorText.text = string.Empty;
            activeErrorText.gameObject.SetActive(false);
        }

        public void ClearSuccessMessage()
        {
            activeSuccesText.text = string.Empty;
            activeSuccesText.gameObject.SetActive(false);
        }

        public void RemoveEnterButtonOnReleaseEvent()
        {
            activeEnterButton.onRelease.RemoveAllListeners();
        }

        public void InsertChar(string c)
        {
            activeInputField.text += c;

            OnUpdate.Invoke(activeInputField.text);
        }

        public void DeleteChar()
        {
            if (activeInputField.text.Length > 0)
            {
                activeInputField.text = activeInputField.text.Substring(0, activeInputField.text.Length - 1);
                OnUpdate.Invoke(activeInputField.text);
            }
        }

        public void InsertSpace()
        {
            activeInputField.text += " ";
            OnUpdate.Invoke(activeInputField.text);
        }

        public void CapsPressed()
        {
            if (!caps)
            {
                normalButtons.SetActive(false);
                capsButtons.SetActive(true);
                caps = true;
            }
            else
            {
                capsButtons.SetActive(false);
                normalButtons.SetActive(true);
                caps = false;
            }
        }

        public void ClearText()
        {
            activeInputField.text = string.Empty;
            ClearErrorMessage();
            ClearSuccessMessage();
            OnUpdate.Invoke(activeInputField.text);
        }
    }
}

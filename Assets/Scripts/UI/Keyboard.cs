using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

namespace EasyMeshVR.UI
{
    // TODO: make the keyboard interactable (can be moved around/rotated with your hands)
    public class Keyboard : MonoBehaviour
    {
        public TMP_InputField activeInputField;
        public TMP_InputField defaultPanelInputField;
        public TMP_InputField importModelPanelInputField;

        public GameObject defaultPanel;
        public GameObject importModelPanel;
        public TMP_Text errorText;
        public TMP_Text successText;
        public GameObject normalButtons;
        public GameObject capsButtons;
        public ButtonVR enterButton;
        public bool caps;

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
            // Rotate the keyboard around its y-axis based on the mainCamera's y rotation
            transform.rotation = Quaternion.identity;
            transform.RotateAround(transform.position, Vector3.up, mainCameraTransform.rotation.eulerAngles.y);

            // Posiiton the keyboard in front of the mainCamera
            Vector3 forwardVec = Vector3.Scale(0.4f * Vector3.one, transform.rotation * Vector3.forward);
            transform.position = mainCameraTransform.position + new Vector3(0f, -0.6f, 0f);
            transform.position += forwardVec;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void DisplayImportModelPanel()
        {
            activeInputField = importModelPanelInputField;
            defaultPanel.SetActive(false);
            importModelPanel.SetActive(true);
        }

        public void DisplayDefaultPanel()
        {
            activeInputField = defaultPanelInputField;
            defaultPanel.SetActive(true);
            importModelPanel.SetActive(false);
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
            enterButton.onRelease.AddListener(onReleaseAction);
        }

        public void AddEnterButtonOnReleaseEvent(Action<string> onReleaseAction)
        {
            enterButton.onRelease.AddListener(delegate
            {
                HandleImportModel(onReleaseAction);
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

        public void DisplayErrorMessage(string errorMsg)
        {
            errorText.text = errorMsg;
            errorText.gameObject.SetActive(true);
        }

        public void DisplaySuccessMessage(string successMsg)
        {
            successText.text = successMsg;
            successText.gameObject.SetActive(true);
        }

        public void ClearErrorMessage()
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }

        public void ClearSuccessMessage()
        {
            successText.text = string.Empty;
            successText.gameObject.SetActive(false);
        }

        public void RemoveEnterButtonOnReleaseEvent()
        {
            enterButton.onRelease.RemoveAllListeners();
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
            OnUpdate.Invoke(activeInputField.text);
        }
    }
}

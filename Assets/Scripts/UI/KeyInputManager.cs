using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

namespace EasyMeshVR.UI
{
    public class KeyInputManager : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Keyboard keyboard;

        private TMP_InputField inputField;

        public static KeyInputManager instance { get; private set; }

        #endregion

        #region MonoBehaviour Callbacks
        void Awake()
        {
            instance = this;
        }

        #endregion

        #region Public Methods

        public void InputFieldSelected(TMP_InputField inputField)
        {
            this.inputField = inputField;
            keyboard.SetText(inputField.text);
            keyboard.Enable();
        }

        public void InputFieldDeselected(TMP_InputField inputField)
        {
            this.inputField = null;
            keyboard.SetText(string.Empty);
            keyboard.Disable();
        }

        public void AddButtonOnClick(Button button)
        {
            UnityAction action = delegate
            {
                button.onClick.Invoke();
            };

            keyboard.AddEnterButtonOnReleaseEvent(action);
        }

        public void DisplayErrorMessage(string errorMsg)
        {
            keyboard.DisplayErrorMessage(errorMsg);
        }

        public void DisplaySuccessMessage(string successMsg)
        {
            keyboard.DisplaySuccessMessage(successMsg);
        }

        public void RemoveButtonOnClick()
        {
            keyboard.RemoveEnterButtonOnReleaseEvent();
        }

        public void UpdateTextField(string text)
        {
            if (inputField != null)
            {
                inputField.text = text;
            }
        }

        public void Submit()
        {
            if (inputField != null)
            {
                DeselectInputField();
            }

            keyboard.Disable();
        }

        public void Cancel()
        {
            if (inputField != null)
            {
                DeselectInputField();
            }

            keyboard.Disable();
        }

        public void EnableKeyboardForImportingModel(Action<string> callback)
        {
            keyboard.Enable();
            keyboard.DisplayImportModelPanel();
            keyboard.AddEnterButtonOnReleaseEvent(callback);
        }

        #endregion

        #region Private Method

        private void DeselectInputField()
        {
            EventSystem eventSystem = EventSystem.current;

            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        #endregion
    }
}

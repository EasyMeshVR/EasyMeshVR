using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
                Submit();
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
            keyboard.ClearText();

            if (inputField != null)
            {
                inputField.ReleaseSelection();
            }
        }

        public void Cancel()
        {
            keyboard.ClearText();
            keyboard.ClearErrorMessage();
            keyboard.ClearSuccessMessage();

            if (inputField != null)
            {
                inputField.ReleaseSelection();
            }

            keyboard.Disable();
        }

        public void EnableKeyboardForImportingModel(Action<string> callback)
        {
            // TODO: enable keyboard numpad layout and a custom background canvas
            keyboard.Enable();
            keyboard.DisplayImportModelPanel();
            keyboard.AddEnterButtonOnReleaseEvent(callback);
        }

        #endregion
    }
}

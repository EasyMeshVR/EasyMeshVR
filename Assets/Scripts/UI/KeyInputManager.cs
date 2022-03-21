using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        public void UpdateTextField(string text)
        {
            if (inputField != null)
            {
                inputField.text = text;
            }
        }

        public void Submit()
        {
            this.inputField = null;
            keyboard.SetText(string.Empty);
        }

        #endregion
    }
}

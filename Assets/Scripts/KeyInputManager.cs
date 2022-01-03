using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRKeys;

namespace EasyMeshVR.UI
{
    public class KeyInputManager : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Keyboard keyboard;

        private TMP_InputField inputField;

        private static KeyInputManager instance;

        private KeyInputManager() { }

        #endregion

        #region MonoBehaviour Callbacks
        void Awake()
        {
            instance = this;
        }

        void Start()
        {

        }

        #endregion

        #region Public Methods

        public static KeyInputManager GetInstance()
        {
            return instance;
        }

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
            else
            {
                Debug.LogError("Attempted to change the text of an inputField without having a reference!");
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

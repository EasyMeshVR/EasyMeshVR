using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyMeshVR.UI
{
    public class InputField : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Button button;

        private TMP_InputField inputField;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        #endregion

        #region Public Methods

        public void InputFieldSelected()
        {
            KeyInputManager.instance.InputFieldSelected(inputField);

            if (button != null)
            {
                KeyInputManager.instance.AddButtonOnClick(button);
            }
        }

        public void InputFieldDeselected()
        {
            KeyInputManager.instance.InputFieldDeselected(inputField);
            KeyInputManager.instance.RemoveButtonOnClick();
        }

        public void DestroyCaret()
        {
            TMP_SelectionCaret caret = inputField.GetComponentInChildren<TMP_SelectionCaret>();

            if (caret && caret.gameObject)
            {
                Destroy(caret.gameObject);
            }
        }

        #endregion
    }
}

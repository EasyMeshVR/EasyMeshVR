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

        // TODO: we can give the KeyInputManager the position of the cursor in the input field
        // so that the user can insert characters in-between the string, currently the VRKeys.Keyboard
        // class just appends to the current string when you type

        // Also TODO: lets use the free Oculus hand models and animate the hands transitioning to a pointer state
        // with the index finger when hovering over a canvas UI (casting a ray/line from the index finger)
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

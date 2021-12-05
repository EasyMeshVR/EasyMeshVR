using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace EasyMeshVR.UI
{
    public class InputField : MonoBehaviour
    {
        #region Private Fields

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
            KeyInputManager.GetInstance().InputFieldSelected(inputField);
        }

        public void InputFieldDeselected()
        {
            KeyInputManager.GetInstance().InputFieldDeselected(inputField);
        }

        #endregion
    }
}

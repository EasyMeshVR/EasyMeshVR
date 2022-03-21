using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

namespace EasyMeshVR.UI
{
    public class Keyboard : MonoBehaviour
    {
        // public GameObject XROrigin;
        public TMP_InputField inputField;
        public GameObject normalButtons;
        public GameObject capsButtons;
        public ButtonVR enterButton;
        public bool caps;

        [Serializable]
        public class KeyboardUpdateEvent : UnityEvent<string> { }

        [Serializable]
        public class KeyboardSubmitEvent : UnityEvent<string> { }

        public KeyboardUpdateEvent OnUpdate = new KeyboardUpdateEvent();
        public KeyboardSubmitEvent OnSubmit = new KeyboardSubmitEvent();

        private Transform mainCameraTransform;

        // Start is called before the first frame update
        void Start()
        {
            caps = false;
        }

        void OnEnable()
        {
            mainCameraTransform = Camera.main.transform;
            transform.position = mainCameraTransform.transform.position + new Vector3(0f, -0.65f, 0.4f);
            //transform.RotateAround(transform.position, mainCameraTransform.up, mainCameraTransform.rotation.eulerAngles.y);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            inputField.text = text;

            OnUpdate.Invoke(inputField.text);
        }

        public void Submit()
        {
            OnSubmit.Invoke(inputField.text);
        }

        public void AddEnterButtonOnReleaseEvent(UnityAction onReleaseAction)
        {
            enterButton.onRelease.AddListener(onReleaseAction);
        }

        public void InsertChar(string c)
        {
            inputField.text += c;

            OnUpdate.Invoke(inputField.text);
        }

        public void DeleteChar()
        {
            if (inputField.text.Length > 0)
            {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                OnUpdate.Invoke(inputField.text);
            }
        }

        public void InsertSpace()
        {
            inputField.text += " ";
            OnUpdate.Invoke(inputField.text);
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
            inputField.text = string.Empty;
            OnUpdate.Invoke(inputField.text);
        }
    }
}

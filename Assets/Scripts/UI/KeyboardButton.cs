using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace EasyMeshVR.UI
{
    public class KeyboardButton : MonoBehaviour
    {
        [SerializeField] private Keyboard keyboard;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private ButtonVR button;

        // Start is called before the first frame update
        void Start()
        {
            if (buttonText.text.Length == 1 && !buttonText.text.Equals("X"))
            {
                NameToButtonText();
                button.onRelease.AddListener(delegate
                {
                    keyboard.InsertChar(buttonText.text);
                });
            }
        }

        public void NameToButtonText()
        {
            buttonText.text = gameObject.name;
        }
    }
}

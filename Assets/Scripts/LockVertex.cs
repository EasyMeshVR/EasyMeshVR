using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;


public class LockVertex : MonoBehaviour
{
   [SerializeField] InputDeviceCharacteristics controllerCharacteristics;
    [SerializeField] XRGrabNetworkInteractable grabInteractable;

    [SerializeField] MoveVertices moveVertices;


    public InputActionReference secondaryButtonRef = null;
    private bool buttonPressed = false;

    [SerializeField] Material locked;     // red
    [SerializeField] Material unselected;   // gray

    [SerializeField] Material hovered;      // orange

    MeshRenderer materialSwap;
   public bool isLocked = false;
   private bool hover = false;


   void OnEnable()
    {
        // Hover listeners to change vertex color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);

        

        materialSwap = GetComponent<MeshRenderer>();

    }

     void HoverOver(HoverEnterEventArgs arg0)
    {
      
        if(!isLocked)
            materialSwap.material = hovered;

        hover = true;
        //();
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
       // if(isLocked)
           // materialSwap.material = locked;

        if(!isLocked)
            materialSwap.material = unselected;

        hover = false;
    }

    private void Awake()
    {
        secondaryButtonRef.action.started += buttonStart;
        secondaryButtonRef.action.canceled += buttonEnd;
    }

    private void OnDestroy()
    {
        secondaryButtonRef.action.started -= buttonStart;
        secondaryButtonRef.action.canceled -= buttonEnd;
    }

    
    private void buttonStart(InputAction.CallbackContext context)
    {
        buttonPressed = true;
        if(!isLocked && hover)
            {
                //moveVertices.enabled = false;
                materialSwap.material = locked;
                gameObject.layer = 2;

                isLocked = true;
                return;
            }
            else if(isLocked && hover)
            {
                //moveVertices.enabled = true;
                materialSwap.material = hovered;
                gameObject.layer = 0;

                isLocked = false;
                return;
            }
    }

    private void buttonEnd(InputAction.CallbackContext context)
    {
        buttonPressed = false;
    }

  

}

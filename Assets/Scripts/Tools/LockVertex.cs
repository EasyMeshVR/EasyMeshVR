using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;


public class LockVertex : MonoBehaviour
{
    [SerializeField] XRGrabNetworkInteractable grabInteractable;

    [SerializeField] MoveVertices moveVertices;

    public InputActionReference primaryButtonref = null;


    public InputActionReference secondaryButtonRef = null;
    private bool primaryButtonPressed = false;

    private bool secondaryButtonPressed = false;


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
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        if(!isLocked)
            materialSwap.material = unselected;

        hover = false;
    }

    private void Awake()
    {
        primaryButtonref.action.started += primaryButtonStart;
        primaryButtonref.action.canceled += primaryButtonEnd;

        secondaryButtonRef.action.started += secondaryButtonStart;
        secondaryButtonRef.action.canceled += secondaryButtonEnd;
    }

    private void OnDestroy()
    {
        primaryButtonref.action.started -= primaryButtonStart;
        primaryButtonref.action.canceled -= primaryButtonEnd;

        secondaryButtonRef.action.started -= secondaryButtonStart;
        secondaryButtonRef.action.canceled -= secondaryButtonEnd;
    }

    
    private void primaryButtonStart(InputAction.CallbackContext context)
    {
        print("primary start");
        primaryButtonPressed = true;
        if(!isLocked && hover)
            {
                materialSwap.material = locked;
                gameObject.layer = 2;

                isLocked = true;
                return;
            }
    }

    private void primaryButtonEnd(InputAction.CallbackContext context)
    {
        primaryButtonPressed = false;
    }

    private void secondaryButtonStart(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = true;
       
        if(isLocked)
            {
                //moveVertices.enabled = true;
                materialSwap.material = unselected;
                gameObject.layer = 0;

                isLocked = false;
                return;
            }
            
    }

    private void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

  

}
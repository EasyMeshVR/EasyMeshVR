using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;


public class LockVertex : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;

    PulleyLocomotion pulleyLocomotion;
    GameObject editingSpace;

    // Other method for locking is to disable moveVertices, but this allows the vertex handle to be grabbable
    //[SerializeField] MoveVertices moveVertices;

    // Primary and secondary buttons on right hand controller (A and B on Oculus)
    public InputActionReference primaryButtonref = null;
    public InputActionReference secondaryButtonRef = null;
    private bool primaryButtonPressed = false;

    private bool secondaryButtonPressed = false;

    // For tool manager, tools should not be active by default
    // disabling the script only disables start() and update() or something like that
    public bool isEnabled = false;


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

        editingSpace = MeshRebuilder.instance.editingSpace;
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();
    }

    // Uncomment materialswapping for disabling/enabling movevertices
     void HoverOver(HoverEnterEventArgs arg0)
    {
      
        //if(!isLocked)
          //  materialSwap.material = hovered;

        // Vertex needs to be hovered over to be locked
        hover = true;
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        //if(!isLocked)
          //  materialSwap.material = unselected;

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

    // Lock vertex on primary button press
    private void primaryButtonStart(InputAction.CallbackContext context)
    {      
        primaryButtonPressed = true;
        if(isEnabled && !pulleyLocomotion.isGrippedL)
        {
            if(!isLocked && hover)
            {
                //moveVertices.enabled = true;

                // Disabling the grab interactable also disables the hovering,
                // I think the only way to allow the hovering would be to implement a hoverInteractable
                // script like the GrabInteractable one that is given but that seems like too much work
                grabInteractable.enabled = false;
                materialSwap.material = locked;

                // This was another way to disable grabbing that I was trying that I forgot about
                // but I'm pretty sure it does the same thing as disabling grabInteractable
                    
                //gameObject.layer = 2;

                isLocked = true;
                return;
            }
        }
    }

    private void primaryButtonEnd(InputAction.CallbackContext context)
    {
        primaryButtonPressed = false;
    }

    // Unlock all locked vertices on secondary button press
    private void secondaryButtonStart(InputAction.CallbackContext context)
    {
       secondaryButtonPressed = true;
       if(isEnabled)
       {
            if(isLocked && !pulleyLocomotion.isMovingEditingSpace)
            {
                //moveVertices.enabled = true;

                // gameObject.layer = 0;

                grabInteractable.enabled = true;
                materialSwap.material = unselected;
                isLocked = false;
                return;
            }
       }
            
    }

    private void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

}
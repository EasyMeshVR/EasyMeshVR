using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;


public class ToolClass : MonoBehaviour
{
    [SerializeReference] public InputActionReference primaryButtonref;
    [SerializeReference] public InputActionReference secondaryButtonRef;

    [SerializeReference] public InputActionReference rightTrigger;

    [SerializeReference] public bool primaryButtonPressed = false;
    [SerializeReference] public bool secondaryButtonPressed = false;
    [SerializeReference] public bool rightTriggerPressed = false;


    // For tool manager, tools should not be active by default
    [SerializeReference] public bool isEnabled = false;

    public void Awake()
    {
        primaryButtonref.action.started += primaryButtonStart;
        primaryButtonref.action.canceled += primaryButtonEnd;

        secondaryButtonRef.action.started += secondaryButtonStart;
        secondaryButtonRef.action.canceled += secondaryButtonEnd;

        rightTrigger.action.started += triggerStart;
        rightTrigger.action.canceled += triggerEnd;

    }

    public void OnDestroy()
    {
        primaryButtonref.action.started -= primaryButtonStart;
        primaryButtonref.action.canceled -= primaryButtonEnd;

        secondaryButtonRef.action.started -= secondaryButtonStart;
        secondaryButtonRef.action.canceled -= secondaryButtonEnd;

        rightTrigger.action.started -= triggerStart;
        rightTrigger.action.canceled -= triggerEnd;
    }

    // Called when primary button pressed
    public virtual void primaryButtonStart(InputAction.CallbackContext context)
    {      
        primaryButtonPressed = true;
        if(isEnabled)
        {
            PrimaryAction();
        }
    }

    // called when primary button released
    public virtual void primaryButtonEnd(InputAction.CallbackContext context)
    {
        primaryButtonPressed = false;
    }

    // Called on first button press of secondary button
    public virtual void secondaryButtonStart(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = true;
        if(isEnabled)
        {

            SecondaryAction();
        }
    }

    // called on release of secondary button
    public virtual void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

    public virtual void triggerStart(InputAction.CallbackContext context)
    {      
        rightTriggerPressed = true;
        if(isEnabled)
        {
            triggerAction();
        }
    }

    public virtual void triggerEnd(InputAction.CallbackContext context)
    {
        rightTriggerPressed = false;
    }

    // action on primary press
    public virtual void PrimaryAction()
    {
        //print("primary parent");
    }

    // action on secondary press
    public virtual void SecondaryAction()
    {
        //print("secondary parent");
    }

    public virtual void triggerAction()
    {
        //print("trigger");
    }

    public virtual void Enable()
    {
        isEnabled = true;
    }

    public virtual void Disable()
    {
        isEnabled = false;
    }
}
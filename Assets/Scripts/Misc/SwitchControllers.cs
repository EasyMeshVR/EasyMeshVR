using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;


// Switch to either type of control
// If on grab, switch to ray for menu
public class SwitchControllers : MonoBehaviour
{
    [SerializeField] InputActionReference startButton;
    [SerializeField] GameObject rayLeft;
    [SerializeField] GameObject rayRight;
    [SerializeField] GameObject grabLeft;
    [SerializeField] GameObject grabRight;

    [SerializeField] ControllersMidpoint rayMidpoint;
    [SerializeField] ControllersMidpoint grabMidpoint;

    

    public bool menuOpen = false;
    public bool rayActive = false;

    void Awake()
    {
        startButton.action.started += startButtonAction;
        startButton.action.canceled += startButtonEnd;;
    }

    void OnDestroy()
    {
        startButton.action.started -= startButtonAction;
        startButton.action.canceled -= startButtonEnd;;
    }

    // only change right hand since menu is still on left
    void startButtonAction(InputAction.CallbackContext context)
    {
        // Don't switch if player is already using raycast
        if(!rayActive)
        {
            if(!menuOpen)
            {
                rayRight.SetActive(true);
                grabRight.SetActive(false);
                menuOpen = true;
                return;
            }
            else
            {
                rayRight.SetActive(false);
                grabRight.SetActive(true);
                menuOpen = false;
                return;
            }
        }
    }

    void startButtonEnd(InputAction.CallbackContext context)
    {
        
    }

    // For enabling either type of control
    void switchToRay()
    {
        rayLeft.SetActive(true);
        rayRight.SetActive(true);

        grabLeft.SetActive(false);
        grabRight.SetActive(false);

        // im not sure if there are going to me multiple of these in a scene at any point but if there are then this will work
        PulleyLocomotion [] list = GameObject.FindObjectsOfType<PulleyLocomotion>(); 
        foreach(PulleyLocomotion pl in list)
            pl.ControllersMidpointObject = rayMidpoint;

        rayActive = true;
    }

    
    
      void switchtoGrab()
    {
        rayLeft.SetActive(false);
        rayRight.SetActive(false);

        grabLeft.SetActive(true);
        grabRight.SetActive(true);

        PulleyLocomotion [] list = GameObject.FindObjectsOfType<PulleyLocomotion>(); 
        foreach(PulleyLocomotion pl in list)
            pl.ControllersMidpointObject = grabMidpoint;

        rayActive = false;
    }

    // just for testing
    void Update()
    {
        if(rayActive)
            switchToRay();
    }
}

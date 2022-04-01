using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;


// Switch to either type of control
// If on grab, switch to ray for menu
public class SwitchControllers : MonoBehaviour
{
    public static SwitchControllers instance { get; private set; }

    [SerializeField] InputActionReference startButton;
    [SerializeField] GameObject rayLeft;
    [SerializeField] GameObject rayRight;
    [SerializeField] GameObject grabLeft;
    [SerializeField] GameObject grabRight;

    [SerializeField] ControllersMidpoint rayMidpoint;
    [SerializeField] ControllersMidpoint grabMidpoint;

    [SerializeField]
    public GameObject activeLeftController;

    [SerializeField]
    public GameObject activeRightController;

    public bool menuOpen = false;
    public bool rayActive = false;

    void Awake()
    {
        instance = this;
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
                activeRightController = rayRight;
                return;
            }
            else
            {
                rayRight.SetActive(false);
                grabRight.SetActive(true);
                activeRightController = grabRight;
                menuOpen = false;
                return;
            }
        }
    }

    void startButtonEnd(InputAction.CallbackContext context)
    {
        
    }

    // For enabling either type of control
    public void switchToRay()
    {
        rayLeft.SetActive(true);
        rayRight.SetActive(true);

        grabLeft.SetActive(false);
        grabRight.SetActive(false);

        activeLeftController = rayLeft;
        activeRightController = rayRight;

        // im not sure if there are going to be multiple of these in a scene at any point but if there are then this should work
        PulleyLocomotion [] list = GameObject.FindObjectsOfType<PulleyLocomotion>(); 
        foreach(PulleyLocomotion pl in list)
        {
            pl.ControllersMidpointObject = rayMidpoint;
            pl.LeftController = rayLeft;
            pl.RightController = rayRight;
        }

        rayActive = true;
    }

    public void switchToGrab()
    {
        rayLeft.SetActive(false);
        rayRight.SetActive(false);

        grabLeft.SetActive(true);
        grabRight.SetActive(true);

        activeLeftController = grabLeft;
        activeRightController = grabRight;

        PulleyLocomotion [] list = GameObject.FindObjectsOfType<PulleyLocomotion>(); 
        foreach(PulleyLocomotion pl in list)
        {
            pl.ControllersMidpointObject = grabMidpoint;
            pl.LeftController = grabLeft;
            pl.RightController = grabRight;
        }

        rayActive = false;
    }

    // just for testing
    void Update()
    {
        if(rayActive)
            switchToRay();
    }
}

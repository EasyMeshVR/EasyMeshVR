using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;


// Switch right hand to ray cast to interact w/ menu
public class SwitchControllers : MonoBehaviour
{

    [SerializeField] InputActionReference startButton;
    GameObject rayRight;

    GameObject grabRight;

    List<GameObject> onlyInactive;
    public bool menuOpen = false;
    void Start()
    {
        // This is the only way to get inactive gameobjects apparently
        onlyInactive =   GameObject.FindObjectsOfType<GameObject>(true).Where(sr => !sr.gameObject.activeInHierarchy &&  sr.CompareTag("RightController")).ToList();   
        foreach (GameObject child in onlyInactive)
            if (child.CompareTag("RightController"))
                rayRight = child;
        
        grabRight = GameObject.Find("RightRadius");
    }

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

    void startButtonAction(InputAction.CallbackContext context)
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

    void startButtonEnd(InputAction.CallbackContext context)
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;



public class SwitchControllers : MonoBehaviour
{

    [SerializeField] InputActionReference startButton;

    GameObject rayLeft;
    GameObject rayRight;

    GameObject grabLeft;
    GameObject grabRight;

    List<GameObject> onlyInactive;
    // Start is called before the first frame update
    public bool menuOpen = false;
    void Start()
    {
        
        onlyInactive =   GameObject.FindObjectsOfType<GameObject>(true).Where(sr => !sr.gameObject.activeInHierarchy && (sr.CompareTag("LeftController") || sr.CompareTag("RightController"))).ToList();   

        print(onlyInactive.Count);
        foreach (GameObject child in onlyInactive)
        {
            if (child.gameObject.tag == "LeftController")
                rayLeft = child;
            if (child.gameObject.tag == "RightController")
                rayRight = child;
        }

        grabLeft = GameObject.Find("LeftHand Controller DirectGrab");
        grabRight = GameObject.Find("RightHand Controller DirectGrab");
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
            rayLeft.SetActive(true);
            rayRight.SetActive(true);
            grabLeft.SetActive(false);
            grabRight.SetActive(false);
            menuOpen = true;
            return;
        }
        else
        {
            rayLeft.SetActive(false);
            rayRight.SetActive(false);
            grabLeft.SetActive(true);
            grabRight.SetActive(true);
            menuOpen = false;
            return;
        }
    }

    void startButtonEnd(InputAction.CallbackContext context)
    {
        
    }
}

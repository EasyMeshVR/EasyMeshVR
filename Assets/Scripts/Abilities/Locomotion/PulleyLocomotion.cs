using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PulleyLocomotion : MonoBehaviour
{
    public InputActionReference lGrabReference = null;
    public InputActionReference rGrabReference = null;
    public InputActionReference flipYLock = null;

    private bool isGrippedL = false;
    private bool isGrippedR = false;

    [SerializeField] private ControllersMidpoint ControllersMidpointObject;
    [SerializeField] private Transform EditingSpaceTF;

    private void Awake()
    {
        if (!ControllersMidpointObject || !EditingSpaceTF)
            Debug.LogError("PulleyLocomotion component needs to be properly filled in inspector!");
        lGrabReference.action.started += LGrabStart;
        lGrabReference.action.canceled += LGrabEnd;
        rGrabReference.action.started += RGrabStart;
        rGrabReference.action.canceled += RGrabEnd;
        flipYLock.action.canceled += FlipYLock;
    }

    private void OnDestroy()
    {
        lGrabReference.action.started -= LGrabStart;
        lGrabReference.action.canceled -= LGrabEnd;
        rGrabReference.action.started -= RGrabStart;
        rGrabReference.action.canceled -= RGrabEnd;
        flipYLock.action.canceled -= FlipYLock;
    }

    private void LGrabStart(InputAction.CallbackContext context)
    {
        isGrippedL = true;
        if (isGrippedR) // Both grips active, use midpoint
        {
            gameObject.transform.parent = ControllersMidpointObject.transform;
        }
    }

    private void RGrabStart(InputAction.CallbackContext context)
    {
        isGrippedR = true;
        if (isGrippedL) // Both grips active, use midpoint
        {
            gameObject.transform.parent = ControllersMidpointObject.transform;
        }
    }

    private void LGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedL = false;
        gameObject.transform.parent = null;
    }

    private void RGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedR = false;
        gameObject.transform.parent = null;
    }

    private void FlipYLock(InputAction.CallbackContext context)
    {
        ControllersMidpointObject.FlipYLock();
    }

    private void ScaleAround(Transform targetTF, Vector3 pivot, Vector3 newScale)
    {
        Vector3 A = targetTF.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / targetTF.localScale.x; // relataive scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        targetTF.localScale = newScale;
        targetTF.localPosition = FP;
    }
}


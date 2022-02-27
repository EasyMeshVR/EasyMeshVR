using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PulleyLocomotion : MonoBehaviour
{
    public InputActionReference lGrabReference = null;
    public InputActionReference rGrabReference = null;
    public InputActionReference flipYLock = null;

    // Set by MoveVertices to disable locomotion when actively pulling a vertex
    public bool isMovingVertex = false;
    public bool isMovingEditingSpace { get; private set; } = false;

    public bool isGrippedL = false;
    public bool isGrippedR = false;
    private Vector3 originalPos; // Temporary original position of editing space in reference to controller

    [SerializeField] public ControllersMidpoint ControllersMidpointObject;
    [SerializeField] public GameObject LeftController;
    [SerializeField] public GameObject RightController;
    [SerializeField] private bool lockRotationAroundYAxis = true;

    private void Awake()
    {
        if (!ControllersMidpointObject || !LeftController || !RightController)
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

    private void Update()
    {
        if (isGrippedL && isGrippedR) // Both hands gripped: trans, rotate, scale
        {
            isMovingEditingSpace = true;
            if (lockRotationAroundYAxis) // Locks rotation around Y axis
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
        else if (isGrippedL && !isGrippedR) // Left hand gripped: translate
        {
            transform.position = originalPos + LeftController.transform.position;
        }
        else if (!isGrippedL && isGrippedR) // Right hand gripped: translate
        {
            transform.position = originalPos + RightController.transform.position;
        }
    }

    private void LGrabStart(InputAction.CallbackContext context)
    {
        if (isMovingVertex)
            return;

        isGrippedL = true;
        if (isGrippedR) // Both grips active, parent to midpoint
            transform.parent = ControllersMidpointObject.transform;
        else
            originalPos = -LeftController.transform.position + transform.position;
    }

    private void RGrabStart(InputAction.CallbackContext context)
    {
        if (isMovingVertex)
            return;

        isGrippedR = true;
        if (isGrippedL) // Both grips active, parent to midpoint
            transform.parent = ControllersMidpointObject.transform;
        else
            originalPos = -RightController.transform.position + transform.position;
    }

    private void LGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedL = false;
        isMovingEditingSpace = false;
        transform.parent = null;
        if (isGrippedR) // If the other grip is still active, go back to translation only
            originalPos = -RightController.transform.position + transform.position;
    }

    private void RGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedR = false;
        isMovingEditingSpace = false;
        gameObject.transform.parent = null;
        if (isGrippedL) // If the other grip is still active, go back to translation only
            originalPos = -LeftController.transform.position + transform.position;
    }

    private void FlipYLock(InputAction.CallbackContext context)
    {
        if (isGrippedL & isGrippedR)
            lockRotationAroundYAxis = !lockRotationAroundYAxis;
    }
}


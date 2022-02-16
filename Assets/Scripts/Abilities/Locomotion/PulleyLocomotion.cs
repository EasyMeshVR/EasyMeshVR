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

    [SerializeField] public ControllersMidpoint ControllersMidpointObject;
    [SerializeField] private Transform EditingSpaceTF;
    [SerializeField] private bool lockRotationAroundYAxis = true;

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

    private void Update()
    {
        if (isGrippedL && isGrippedR)
        {
            isMovingEditingSpace = true;
            if (lockRotationAroundYAxis) // Locks rotation around Y axis
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    private void LGrabStart(InputAction.CallbackContext context)
    {
        if (isMovingVertex)
            return;

        isGrippedL = true;
        if (isGrippedR) // Both grips active, parent to midpoint
            transform.parent = ControllersMidpointObject.transform;
    }

    private void RGrabStart(InputAction.CallbackContext context)
    {
        if (isMovingVertex)
            return;

        isGrippedR = true;
        if (isGrippedL) // Both grips active, parent to midpoint
            transform.parent = ControllersMidpointObject.transform;
    }

    private void LGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedL = false;
        isMovingEditingSpace = false;
        transform.parent = null;
    }

    private void RGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedR = false;
        isMovingEditingSpace = false;
        gameObject.transform.parent = null;
    }

    private void FlipYLock(InputAction.CallbackContext context)
    {
        if (isGrippedL & isGrippedR)
            lockRotationAroundYAxis = !lockRotationAroundYAxis;
    }
}


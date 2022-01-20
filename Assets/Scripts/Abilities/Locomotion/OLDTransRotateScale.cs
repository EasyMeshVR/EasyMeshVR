using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransRotateScale : MonoBehaviour
{
    public InputActionReference lGrabReference = null;
    public InputActionReference rGrabReference = null;

    private bool isGrippedL = false;
    private bool isGrippedR = false;

    Vector3 midpointPos; // Vector that is the position of the midpoint of the controllers
    Vector3 midpointDir; // Vector from the midpoint of the controllers to right controller
    Vector3 midpointPosInitial; // But at the start of locomotion
    Vector3 midpointDirInitial; // But at the start of locomotion

    [SerializeField] private Transform XRRigTF;
    private Vector3 positionInitial, scaleInitial;
    private Quaternion rotationInitial;
    [SerializeField] private Transform LController;
    [SerializeField] private Transform RController;

    private enum rotationTypes { AroundYAxis, FreeRotation };
    [SerializeField] rotationTypes rotationType = rotationTypes.AroundYAxis;

    private void Awake()
    {
        lGrabReference.action.started += LGrabStart;
        lGrabReference.action.canceled += LGrabEnd;
        rGrabReference.action.started += RGrabStart;
        rGrabReference.action.canceled += RGrabEnd;
    }

    private void OnDestroy()
    {
        lGrabReference.action.started -= LGrabStart;
        lGrabReference.action.canceled -= LGrabEnd;
        rGrabReference.action.started -= RGrabStart;
        rGrabReference.action.canceled -= RGrabEnd;
    }


    private void Update()
    {
        midpointPos = (LController.localPosition + RController.localPosition) / 2;
        midpointDir = (RController.localPosition - LController.localPosition) / 2;

        if (isGrippedL && isGrippedR)
        {
            if (true) // Translate
            {
                XRRigTF.localPosition = positionInitial - (midpointPos - midpointPosInitial);
            }

            if (false) // Rotate
            {

            }

            if (true) // Scale
            {
                // Simply scaling would mean the XRRig is scaled around its center. It should ideally be scaled
                // around the midpoint. To make it appear to scale around the midpoint, its scale and position
                // must be changed.

                //float scaleFactor = midpointDirInitial.magnitude / midpointDir.magnitude;

                //Vector3 postScalePosition = ((LController.position + RController.position) / 2) + ((XRRigTF.localPosition - ((LController.position + RController.position) / 2)) * scaleFactor);

                //XRRigTF.localScale = scaleInitial * (midpointDirInitial.magnitude / midpointDir.magnitude);
                //XRRigTF.localPosition = postScalePosition;
                float scale = (midpointDirInitial.magnitude / midpointDir.magnitude);
                Vector3 pivot = (LController.position + RController.position) / 2;

                ScaleAround(XRRigTF, pivot, new Vector3(scale, scale, scale));
            }
        }
    }

    private void LGrabStart(InputAction.CallbackContext context)
    {
        isGrippedL = true;
        CopyInitialTF();
        if (isGrippedR) // Both grips are active, use midpoint
        {
            midpointPosInitial = (LController.localPosition + RController.localPosition) / 2;
            midpointDirInitial = (RController.localPosition - LController.localPosition) / 2;
        }
    }

    private void RGrabStart(InputAction.CallbackContext context)
    {
        isGrippedR = true;
        CopyInitialTF();
        if (isGrippedL) // Both grips are active, use midpoint
        {
            midpointPosInitial = (LController.localPosition + RController.localPosition) / 2;
            midpointDirInitial = (RController.localPosition - LController.localPosition) / 2;
        }
    }

    private void LGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedL = false;
    }

    private void RGrabEnd(InputAction.CallbackContext context)
    {
        isGrippedR = false;
    }

    /// <summary>
    /// Copies initial values of the XRRig Transform component to some temporary variables
    /// </summary>
    private void CopyInitialTF()
    {
        if (!XRRigTF)
        {
            Debug.LogError("No reference to XRRig's Transform component!");
            return;
        }

        positionInitial = XRRigTF.localPosition;
        rotationInitial = XRRigTF.rotation;
        scaleInitial = XRRigTF.localScale;
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


    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawLine(LController.position, RController.position);
    //    Gizmos.DrawSphere((LController.position + RController.position) / 2, .125f * XRRigTF.localScale.x);
    //}
}

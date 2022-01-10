using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransRotateScale : MonoBehaviour
{

    [SerializeField] private bool doTranslate = false;
    [SerializeField] private bool doRotate = false;
    [SerializeField] private bool doScale = false;

    [SerializeField] private Transform XRRigTF;
    [SerializeField] private Transform LController;
    [SerializeField] private Transform RController;

    private Vector3 transReferencePoint;
    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 inititalScale;

    enum tempTransReferenceTypes { LHand, RHand, BothHands };
    [SerializeField] tempTransReferenceTypes tempTransReference = tempTransReferenceTypes.LHand;

    void Start()
    {
        
    }


    private void Update()
    {
        if (tempTransReference == tempTransReferenceTypes.LHand)
            transReferencePoint = LController.position;
        else if (tempTransReference == tempTransReferenceTypes.RHand)
            transReferencePoint = RController.position;
        else
            transReferencePoint = (LController.position + RController.position) / 2;

        if (doTranslate)
        {
            XRRigTF.position = XRRigTF.position - transReferencePoint;
        }
    }


    /// <summary>
    /// Action to bind to a controller to start translating the player in space
    /// </summary>
    public void BeginTranslate()
    {
        doTranslate = true;
        // TODO: Some sort of switching between left controller, right controller, or midpoint between the two as transform reference
        initialPosition = XRRigTF.position;
    }

    /// <summary>
    /// Action to bind to a controller to stop translating the player in space
    /// </summary>
    public void EndTranslate()
    {
        doTranslate = false;
    }
}

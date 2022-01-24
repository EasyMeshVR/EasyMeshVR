using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maintains the position of the ControllersMidpoint object
/// to be the midpoint between the controllers, and the rotation to
/// be from the left controller to the right controller.
/// </summary>
public class ControllersMidpoint : MonoBehaviour
{
    [SerializeField] private Transform LController;
    [SerializeField] private Transform RController;

    private void Update()
    {

        Vector3 midpointPos = (LController.localPosition + RController.localPosition) / 2;
        Vector3 midpointDir = (LController.localPosition - RController.localPosition) / 2;
        // Maintain Position
        gameObject.transform.localPosition = midpointPos;

        // Maintain Rotation
        //if (lockRotationAroundYAxis)
        //    gameObject.transform.localRotation = Quaternion.LookRotation(new Vector3(midpointDir.x, 0, midpointDir.z), Vector3.up);
        //else
            gameObject.transform.localRotation = Quaternion.LookRotation(midpointDir, LController.up + RController.up);

        // Maintain Scale
        gameObject.transform.localScale = new Vector3(midpointDir.magnitude, midpointDir.magnitude, midpointDir.magnitude);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(LController.position, RController.position);
        Gizmos.DrawSphere((LController.position + RController.position) / 2, .0625f);
    }
}

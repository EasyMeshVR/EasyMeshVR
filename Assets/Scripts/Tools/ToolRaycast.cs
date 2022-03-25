using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For tools with raycast controls get raycast info from this class
// Can probably also get raycast info from XRRayInteractor script but it looks complicated
public class ToolRaycast : MonoBehaviour
{
    public RaycastHit hit;
    public bool hitVertex;
    public bool hitEdge;
    public bool hitFace;
    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * Mathf.Infinity, Color.yellow);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity) && hit.transform.CompareTag("Vertex"))
            hitVertex = true;
        
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity) && hit.transform.CompareTag("Edge"))
            hitEdge = true;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity) && hit.transform.CompareTag("Face"))
            hitFace = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For tools with raycast controls get raycast info from this class
// Can probably also get raycast info from XRRayInteractor script but it looks complicated
public class ToolRaycast : MonoBehaviour
{
    public RaycastHit hit;
    [SerializeField] GameObject RaycastOrigin;
    public bool hitVertex;
    public bool hitEdge;
    public bool hitFace;
    void FixedUpdate()
    {
        Debug.DrawRay(RaycastOrigin.transform.position, transform.TransformDirection(Vector3.forward) * 2.5f, Color.yellow);

        if (Physics.Raycast(RaycastOrigin.transform.position, transform.TransformDirection(Vector3.forward), out hit, 2.5f))
        {
           // print("hit name " + hit.collider.gameObject.name);
           // print("hit tag " + hit.collider.tag);
            if(hit.transform.gameObject.CompareTag("Vertex"))
                hitVertex = true;
            else if (hit.transform.gameObject.CompareTag("Edge"))
                hitEdge = true;
            else if (hit.transform.gameObject.CompareTag("Face"))
                hitFace = true;
            
        }
        
        else
        {
            hitVertex = false;
            hitEdge = false;
            hitFace = false;
        }
    }
}

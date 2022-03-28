using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Core;

// When using radius controllers, only highlight nearest object
public class ChangeMatClosest : MonoBehaviour
{

    [SerializeField] SphereCollider sC;
    [SerializeField] Material hoveredO;
    [SerializeField] Material unselected;
    GameObject nearObject;
    [SerializeField] PulleyLocomotion pm;

    [SerializeField] ChangeMatClosest otherHand;

    MeshRenderer materialSwap;


    private void OnTriggerStay(Collider other)
    { 
        checkImport();

        if(pm.isMovingEditingSpace || pm.isMovingVertex)
            return;


        float nearObjectDistance = 0f;
        Vector3 center = sC.transform.position + sC.center;
    
        // Get all colliders in collision and find closest
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center, sC.radius);
        foreach(Collider c in allOverlappingColliders)
        {
            float curDistance = Vector3.Distance(c.transform.position, sC.transform.position) *.5f;
 
            if ((!nearObject || curDistance < nearObjectDistance) && !c.CompareTag(Constants.GAME_CONTROLLER_TAG))
            {
                nearObjectDistance = curDistance;
                nearObject = c.gameObject;
            }
        }
 
        // Don't do anything if the vertex is locked or if locomotion is happening or if another thing is being pulled
        if(nearObject != null)
        {
            if(nearObject.CompareTag("Vertex") && nearObject.GetComponent<MoveVertices>().isLocked)
            {
                nearObject = null;
                return;
            }

            // Bug if moving edge then grabbing vertex will allow locked edges to be highlighted
            if(otherHand.nearObject!=null)
                if(otherHand.nearObject.CompareTag("Edge") && nearObject.CompareTag("Edge") && otherHand.nearObject.GetComponent<MoveEdge>().grabHeld)
                    return;

            materialSwap = nearObject.GetComponent<MeshRenderer>();

            if (materialSwap == null)
            {
                return;
            }

            materialSwap.material = hoveredO;
        }
     }

    // Change the material back on exit
    public void OnTriggerExit(Collider other)
    {
        checkImport();
        if(nearObject == null)
            return;

    

         if(pm.isMovingEditingSpace || pm.isMovingVertex)
            return;

        if(nearObject.CompareTag("Vertex") && nearObject.GetComponent<MoveVertices>().isLocked)
            return;


       // materialSwap = other.gameObject.GetComponent<MeshRenderer>();

       // materialSwap.material = unselected;
        nearObject = null;
    }

    // this might be wrong
    // Get a new reference to the pulley locomotion script if a new editing space is imported
     void checkImport()
    {
        pm = GameObject.FindObjectOfType<PulleyLocomotion>();
    }
}

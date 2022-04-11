using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Core;

// When using radius controllers, only highlight nearest object
public class ChangeMatClosest : MonoBehaviour
{
    [SerializeField] LayerMask meshInteractableLayerMask;
    [SerializeField] SphereCollider sC;
    [SerializeField] Material hoveredO;
    [SerializeField] Material unselected;
    public GameObject nearObject;
    [SerializeField] PulleyLocomotion pm;

    [SerializeField] ChangeMatClosest otherHand;

    MeshRenderer materialSwap;


    private void OnTriggerEnter(Collider other)
    { 
        checkImport();

        if(pm.isMovingEditingSpace || pm.isMovingVertex)
            return;


        float nearObjectDistance = 0f;
        Vector3 center = sC.transform.position + sC.center;

        // Get all colliders in collision and find closest
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center, sC.radius, meshInteractableLayerMask);

        if (allOverlappingColliders.Length > 10)
        {
            //Debug.Log("Way too many colliders overlapping, skipping this OnTriggerStay call");
            return;
        }

        foreach(Collider c in allOverlappingColliders)
        {
            float curDistance = Vector3.Distance(c.bounds.center, sC.transform.position) *.5f;

            // if ((!nearObject || curDistance < nearObjectDistance) && !c.CompareTag(Constants.GAME_CONTROLLER_TAG))
            if ((!nearObject || curDistance < nearObjectDistance))
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

            // Bug --  if moving edge then grabbing vertex will allow locked edges to be highlighted
            if(otherHand.nearObject!=null)
                if(otherHand.nearObject.CompareTag("Edge") && nearObject.CompareTag("Edge") && otherHand.nearObject.GetComponent<MoveEdge>().grabHeld)
                    return;


            

            materialSwap = nearObject.GetComponent<MeshRenderer>();

            if(nearObject.CompareTag("Edge"))
                if(nearObject.GetComponent<Edge>().locked)
                    return;

            if(nearObject.CompareTag("Face"))
            {
                if(nearObject.GetComponent<Face>().locked)
                    return;
                    
                Face faceComp = nearObject.GetComponent<Face>();
                // don't change material of locked vertices

                // return if anything is locked
                if(faceComp.vertObj1.GetComponent<MoveVertices>().isLocked || faceComp.vertObj2.GetComponent<MoveVertices>().isLocked || faceComp.vertObj3.GetComponent<MoveVertices>().isLocked)
                    return;
               
                materialSwap = faceComp.vertObj1.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;
            
                materialSwap = faceComp.vertObj2.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;
            
                materialSwap = faceComp.vertObj3.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;
                

                // change material of edges

                materialSwap = faceComp.edgeObj1.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;
                
                materialSwap = faceComp.edgeObj2.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;

                materialSwap = faceComp.edgeObj3.GetComponent<MeshRenderer>();
                materialSwap.material = hoveredO;
            }

            if (materialSwap == null)
                return;


            materialSwap.material = hoveredO;


        }
     }

    // Change the material back on exit
    public void OnTriggerExit(Collider other)
    {
        checkImport();
        if(nearObject == null)
            return;

        if(nearObject.CompareTag("Edge"))
            if(nearObject.GetComponent<Edge>().locked)
                return;

         if(pm.isMovingEditingSpace || pm.isMovingVertex)
            return;

        if(nearObject.CompareTag("Vertex") && nearObject.GetComponent<MoveVertices>().isLocked)
            return;

        if(nearObject.CompareTag("Face"))
        {
            if(nearObject.GetComponent<Face>().locked)
                    return;

                Face faceComp = nearObject.GetComponent<Face>();
                // don't change material of locked vertices

                if(faceComp.vertObj1.GetComponent<MoveVertices>().isLocked || faceComp.vertObj2.GetComponent<MoveVertices>().isLocked || faceComp.vertObj3.GetComponent<MoveVertices>().isLocked)
                    return;
               
                materialSwap = faceComp.vertObj1.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;
           
                materialSwap = faceComp.vertObj2.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;
                
                materialSwap = faceComp.vertObj3.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;
                
                // change material of edges

                materialSwap = faceComp.edgeObj1.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;
                
                materialSwap = faceComp.edgeObj2.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;

                materialSwap = faceComp.edgeObj3.GetComponent<MeshRenderer>();
                materialSwap.material = unselected;
        }
        


       // materialSwap = other.gameObject.GetComponent<MeshRenderer>();

       // materialSwap.material = unselected;
        nearObject = null;
    }

    // this might be wrong
    // Get a new reference to the pulley locomotion script if a new editing space is imported
     void checkImport()
    {
        //pm = GameObject.FindObjectOfType<PulleyLocomotion>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMatClosest : MonoBehaviour
{

    [SerializeField] SphereCollider sC;
    [SerializeField] Material hovered;
    [SerializeField] Material unselected;
    GameObject nearObject;
    [SerializeField] PulleyLocomotion pm;
    MeshRenderer materialSwap;


    private void OnTriggerStay(Collider other)
    { 
        checkImport();

        float nearObjectDistance = 0f;
        Vector3 center = sC.transform.position + sC.center;
    
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center, sC.radius);
        foreach(Collider c in allOverlappingColliders)
        {
            float curDistance = Vector3.Distance(c.transform.position, sC.transform.position);

            if ((!nearObject || curDistance < nearObjectDistance) && !c.CompareTag("GameController"))
            {
                nearObjectDistance = curDistance;
                nearObject = c.gameObject;
            }
        }
 
        if(nearObject != null)
        {
            if(nearObject.CompareTag("Vertex") && nearObject.GetComponent<MoveVertices>().isLocked)
            {
                nearObject = null;
                return;
            }

            if(pm.isMovingEditingSpace)
                return;

            materialSwap = nearObject.GetComponent<MeshRenderer>();
            materialSwap.material = hovered;
        }
     }

     public void OnTriggerExit(Collider other)
    {
        checkImport();

        if(nearObject.CompareTag("Vertex") && nearObject.GetComponent<MoveVertices>().isLocked)
            return;

        if(pm.isMovingEditingSpace)
            return;

        materialSwap = other.gameObject.GetComponent<MeshRenderer>();
        materialSwap.material = unselected;
        nearObject = null;
    }

    // this might be wrong
     void checkImport()
    {
        pm = GameObject.FindObjectOfType<PulleyLocomotion>();
    }
}

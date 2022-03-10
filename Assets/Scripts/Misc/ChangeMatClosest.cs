using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMatClosest : MonoBehaviour
{

    [SerializeField] SphereCollider sC;
    [SerializeField] Material hovered;


    MeshRenderer materialSwap;


    private void OnTriggerStay(Collider other)
    { 
        //Collider[] hitColliders = new Collider[100];
        GameObject nearObject = null;
         float nearObjectDistance = 0f;
        Vector3 center = sC.transform.position + sC.center;
        //Vector3 radius = sC.radius;
        //print("center " + center + " radius " + sC.radius);
        //int numColliders = Physics.OverlapSphereNonAlloc(center, sC.radius, hitColliders);
       // print("numColliders" + numColliders);
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center, sC.radius);
        foreach(Collider c in allOverlappingColliders)
        {
            float curDistance = Vector3.Distance(c.transform.position, sC.transform.position);
           // print("gameobject " + c.name + " is " + curDistance + " away from sphere");
           // print(c.name + " tag is " + c.tag);
            if ((!nearObject || curDistance < nearObjectDistance) && !c.CompareTag("GameController"))
            {
                nearObjectDistance = curDistance;
                nearObject = c.gameObject;
            }
        }
        // if(nearObject != null)
        //     print("nearest is " + nearObject.name);
        materialSwap = nearObject.GetComponent<MeshRenderer>();
        materialSwap.material = hovered;




     }
}

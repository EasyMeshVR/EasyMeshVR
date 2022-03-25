using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public int id;
    public int vert1;
    public int vert2;

    public int vert3;

    public int edge1;
    public int edge2;
    public int edge3;
    
    public bool isHeldByOther;
    public int heldByActorNumber = -1;
    public bool locked;

    public Vector3 normal;

    MeshRebuilder meshRebuilder;
    // void Update()
    // {
    //     Debug.DrawRay(transform.position, normal * 180, Color.blue);
    //     print(id + " normal is " + normal);
    // }
}

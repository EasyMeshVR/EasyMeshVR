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

    public Vertex vertObj1;
    public Vertex vertObj2;
    public Vertex vertObj3;

    public Edge edgeObj1;
    public Edge edgeObj2;
    public Edge edgeObj3;

    public GameObject thisFace;



    MeshRebuilder meshRebuilder;
    // void Update()
    // {
    //     Debug.DrawRay(transform.position, normal * 180, Color.blue);
    //     print(id + " normal is " + normal);
    // }
}

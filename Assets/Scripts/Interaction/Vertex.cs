using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public int id;
    public bool isHeldByOther = false;
    public int heldByActorNumber = -1;

    public GameObject thisVertex;
    public List<Edge> connectedEdges = new List<Edge>();
    public List<Face> connectedFaces = new List<Face>();

    // void Update()
    // {
    //     Debug.DrawRay(transform.localPosition, transform.localPosition + transform.forward * 180, Color.red);

    // }
}


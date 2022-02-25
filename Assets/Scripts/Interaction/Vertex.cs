using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public int id;
    public bool isHeldByOther = false;

    public GameObject thisVertex;
    public List<Edge> connectedEdges = new List<Edge>();
}

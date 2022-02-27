using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public int id;
    public int vert1;
    public int vert2;

    public GameObject thisEdge;

    public bool isHeldByOther;
    public bool locked;
}

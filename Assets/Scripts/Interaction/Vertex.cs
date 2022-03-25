using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public int id;
    public bool isHeldByOther = false;
    public int heldByActorNumber = -1;

    // void Update()
    // {
    //     Debug.DrawRay(transform.localPosition, transform.localPosition + transform.forward * 180, Color.red);

    // }
}


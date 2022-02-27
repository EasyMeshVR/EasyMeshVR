using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Resize sphere collider radius based on midpoint
public class ResizeRadius : MonoBehaviour
{
    [SerializeField] private ControllersMidpoint ControllersMidpointObject;
    [SerializeField] SphereCollider sphereCollider;

    Transform editingSpace;

    void Start()
    {
        editingSpace = GameObject.Find("EditingSpace").transform;
    }

    // Resize raidus every frame
    void Update()
    {
        Vector3 scale = ControllersMidpointObject.transform.localScale;

        // Prevents the radius from getting too big relative to the mesh
        if(.25f * editingSpace.localScale.sqrMagnitude <=  scale.sqrMagnitude)
            return;

        sphereCollider.radius = scale.sqrMagnitude;
    }
}
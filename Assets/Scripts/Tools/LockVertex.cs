using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;


public class LockVertex : ToolClass
{
    [SerializeField] Material locked;     // red
    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange

    [SerializeField] SwitchControllers switchControllers;

    [SerializeField] ToolRaycast ray;


    public XRGrabInteractable vertexGrabInteractable;

   // public PulleyLocomotion pulleyLocomotion;
   // public GameObject editingSpace;
    public GameObject currentVertex;
    public GameObject currentEdge;

    public bool inRadius = false;

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;

    int selectedVertex;
    int selectedEdge;
    

    MeshRenderer materialSwap;

    private bool hover = false;
    private float holdTime = 0f;

    bool holdFinish = false;

   void OnEnable()
    {
       // editingSpace = GameObject.Find("EditingSpace");
       // pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        leftSphere = GameObject.Find("LeftRadius").GetComponent<SphereCollider>();
        rightSphere = GameObject.Find("RightRadius").GetComponent<SphereCollider>();
    }

    public override void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
        holdTime = 0f;
    }

    public override void PrimaryAction()
    {
        if(!inRadius)
            return;

        if(currentVertex == null)
            return;
        
        if(currentVertex.GetComponent<MoveVertices>().isLocked)
            return;
        Lock();
    }

    public override void SecondaryAction()
    {
        if(!inRadius)
            return;

        if(currentVertex == null)
            return;
        
        if(!currentVertex.GetComponent<MoveVertices>().isLocked)
            return;

        Unlock();
    }

    // Change material, disable vertex grab interactable, set boolean
    void Lock()
    {
        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        vertexGrabInteractable.enabled = false;
        materialSwap.material = locked;

        currentVertex.GetComponent<MoveVertices>().isLocked = true;
        return;
    }

    // Change material, enbable vertex grab interactable, set boolean
    void Unlock()
    {
        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        vertexGrabInteractable.enabled = true;
        materialSwap.material = unselected;
        currentVertex.GetComponent<MoveVertices>().isLocked = false;
        return;
    }

    // Get vertex info from sphere collision
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vertex"))
        {
            currentVertex = other.gameObject; 
            selectedVertex = currentVertex.GetComponent<Vertex>().id;
            vertexGrabInteractable = currentVertex.GetComponent<XRGrabInteractable>();
            inRadius = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if(!switchControllers.rayActive)
        {
            inRadius = false;
            currentVertex = null;
        }
    }

    // Separate raycast for raycast controllers, gets vertex info from raycast hit
    void Update()
    {
        if(switchControllers.rayActive)
        {
            if(ray.hitVertex)
            {
                currentVertex = ray.hit.transform.gameObject;
                vertexGrabInteractable = currentVertex.GetComponent<XRGrabInteractable>();
                if(primaryButtonPressed)
                    Lock();
                if(secondaryButtonPressed)
                    Unlock();
            }
            else
            {
                inRadius = false;
                currentVertex = null;
            }
        }
    }
}
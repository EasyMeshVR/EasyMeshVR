using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;
using EasyMeshVR.Multiplayer;
using Photon.Pun;

public class LockVertex : ToolClass
{
    [SerializeField] Material locked;     // red
    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange


    [SerializeField] Material lockedEdge;     // red

    [SerializeField] SwitchControllers switchControllers;

    [SerializeField] ToolRaycast ray;

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
        Lock(currentVertex.GetComponent<Vertex>());
    }

    public override void SecondaryAction()
    {
        if(!inRadius)
            return;

        if(currentVertex == null)
            return;
        
        if(!currentVertex.GetComponent<MoveVertices>().isLocked)
            return;

        Unlock(currentVertex.GetComponent<Vertex>());
    }

    // Change material, disable vertex grab interactable, set boolean
    public void Lock(Vertex currentVertex, bool sendVertexLockEvent = true)
    {
        MeshRebuilder meshRebuilder = currentVertex.GetComponent<MoveVertices>().meshRebuilder;

        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        currentVertex.GetComponent<XRGrabInteractable>().enabled = false;
        materialSwap.material = locked;
        currentVertex.GetComponent<MoveVertices>().isLocked = true;

        foreach(Edge e in meshRebuilder.edgeObjects)
        {
            if(e.vert1 == currentVertex.id || e.vert2 == currentVertex.id)
            {
                e.GetComponent<XRGrabInteractable>().enabled = false;
                materialSwap = e.GetComponent<MeshRenderer>();
                // maybe use the pink lock
                materialSwap.material = lockedEdge;
                e.locked = true;
                e.GetComponent<MoveEdge>().isLocked = true;
                // maybe add isLocked to all move scipts
            }
        }

        foreach(Face f in meshRebuilder.faceObjects)
        {
            if(f.vert1 == currentVertex.id || f.vert2 == currentVertex.id  || f.vert3 == currentVertex.id)
            {
                f.GetComponent<XRGrabInteractable>().enabled = false;
                f.GetComponent<MoveFace>().isLocked = true;
                f.locked = true;
            }
        }

        // Only send the event if specified by the bool parameter "sendFaceExtrudeEvent"
        if (sendVertexLockEvent)
        {
            // Synchronize the cached vertex lock event to other players by vertex id
            VertexLockEvent vertexLockEvent = new VertexLockEvent()
            {
                id = currentVertex.id,
                meshId = meshRebuilder.id,
                isCached = true,
                locked = true,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
        }
    }

    // Change material, enbable vertex grab interactable, set boolean
    public void Unlock(Vertex currentVertex, bool sendVertexLockEvent = true)
    {
        MeshRebuilder meshRebuilder = currentVertex.GetComponent<MoveVertices>().meshRebuilder;

        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        currentVertex.GetComponent<XRGrabInteractable>().enabled = true;
        materialSwap.material = unselected;
        currentVertex.GetComponent<MoveVertices>().isLocked = false;

        
        foreach(Edge e in meshRebuilder.edgeObjects)
        {
            if(e.vert1 == currentVertex.id || e.vert2 == currentVertex.id)
            {
                e.GetComponent<XRGrabInteractable>().enabled = true;
                materialSwap = e.GetComponent<MeshRenderer>();
                // maybe use the pink lock
                materialSwap.material = unselected;
                e.locked = false;
                e.GetComponent<MoveEdge>().isLocked = false;
                // maybe add isLocked to all move scipts

            }

        }

        foreach(Face f in meshRebuilder.faceObjects)
        {
            if(f.vert1 == currentVertex.id || f.vert2 == currentVertex.id || f.vert3 == currentVertex.id)
            {
                f.GetComponent<XRGrabInteractable>().enabled = true;
                f.GetComponent<MoveFace>().isLocked = false;
                f.locked = false;
            }
        }

        // Only send the event if specified by the bool parameter "sendFaceExtrudeEvent"
        if (sendVertexLockEvent)
        {
            // Synchronize the cached vertex lock event to other players by vertex id
            VertexLockEvent vertexLockEvent = new VertexLockEvent()
            {
                id = currentVertex.id,
                meshId = meshRebuilder.id,
                isCached = true,
                locked = false,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
        }
    }

    // Get vertex info from sphere collision
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vertex"))
        {
            currentVertex = other.gameObject; 
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

    public override void Disable()
    {
        isEnabled = false;
    }

    public override void Enable()
    {
        isEnabled = true;
    }

    // Separate raycast for raycast controllers, gets vertex info from raycast hit
    void Update()
    {
       // print("lock enabled is " + enabled);
       if(!isEnabled)
                return;
        if(switchControllers.rayActive)
        {
            
            if(ray.hitVertex)
            {
                currentVertex = ray.hit.transform.gameObject;
                if(primaryButtonPressed)
                    Lock(currentVertex.GetComponent<Vertex>());
                if(secondaryButtonPressed)
                    Unlock(currentVertex.GetComponent<Vertex>());
            }
            else
            {
                inRadius = false;
                currentVertex = null;
            }
        }
    }
}
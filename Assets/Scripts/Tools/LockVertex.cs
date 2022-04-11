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
    public GameObject currentObj;
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

        if(currentObj == null)
            return;
        
        if(currentObj.CompareTag("Vertex"))
            if(currentObj.GetComponent<MoveVertices>().isLocked)
                return;

        if(currentObj.CompareTag("Edge"))
            if(currentObj.GetComponent<MoveEdge>().isLocked)
                return;

        if(currentObj.CompareTag("Face"))
            if(currentObj.GetComponent<MoveFace>().isLocked)
                return;

        Lock(currentObj);
    }

    public override void SecondaryAction()
    {
        if(!inRadius)
            return;

        if(currentObj == null)
            return;
        
        if(currentObj.CompareTag("Vertex"))
            if(!currentObj.GetComponent<MoveVertices>().isLocked)
                return;

        if(currentObj.CompareTag("Edge"))
            if(!currentObj.GetComponent<MoveEdge>().isLocked)
                return;

        if(currentObj.CompareTag("Face"))
            if(!currentObj.GetComponent<MoveFace>().isLocked)
                return;

        Unlock(currentObj);
    }

    // Change material, disable vertex grab interactable, set boolean
    public void Lock(GameObject currentObj, bool sendVertexLockEvent = true)
    {
        
        if(currentObj == null)
            return;
        if(currentObj.CompareTag("Vertex"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveVertices>().meshRebuilder;

            materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = false;
            materialSwap.material = locked;
            currentObj.GetComponent<MoveVertices>().isLocked = true;
            Vertex currVert = currentObj.GetComponent<Vertex>();

            foreach(Edge e in meshRebuilder.edgeObjects)
            {
                if(e.vert1 == currVert.id || e.vert2 == currVert.id)
                {
                    e.GetComponent<XRGrabInteractable>().enabled = false;
                    materialSwap = e.GetComponent<MeshRenderer>();
                    materialSwap.material = lockedEdge;
                    e.locked = true;
                    e.GetComponent<MoveEdge>().isLocked = true;
                }
            }

            foreach(Face f in meshRebuilder.faceObjects)
            {
                if(f.vert1 == currVert.id || f.vert2 == currVert.id  || f.vert3 == currVert.id)
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
                    id = currVert.id,
                    meshId = meshRebuilder.id,
                    isCached = true,
                    locked = true,
                    actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
                };

                NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
            }
        }

        if(currentObj.CompareTag("Edge"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveEdge>().meshRebuilder;

            materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = false;
            materialSwap.material = locked;
            currentObj.GetComponent<MoveVertices>().isLocked = true;
            Edge currEdge = currentObj.GetComponent<Edge>();

            foreach(Vertex v in meshRebuilder.vertexObjects)
            {
                if(v.id == currEdge.vert1 || v.id == currEdge.vert2)
                {
                    v.GetComponent<XRGrabInteractable>().enabled = false;
                    materialSwap = v.GetComponent<MeshRenderer>();
                    materialSwap.material = locked;
                    v.GetComponent<MoveVertices>().isLocked = true;
                }
            }

            foreach(Face f in meshRebuilder.faceObjects)
            {
                if(f.edge1 == currEdge.id || f.edge2 == currEdge.id  || f.edge3 == currEdge.id)
                {
                    f.GetComponent<XRGrabInteractable>().enabled = false;
                    f.GetComponent<MoveFace>().isLocked = true;
                    f.locked = true;
                }
            }

            // if (sendVertexLockEvent)
            // {
            //     // Synchronize the cached vertex lock event to other players by vertex id
            //     VertexLockEvent vertexLockEvent = new VertexLockEvent()
            //     {
            //         id = currEdge.id,
            //         meshId = meshRebuilder.id,
            //         isCached = true,
            //         locked = true,
            //         actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            //     };

            //     NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
            // }
        }

        if(currentObj.CompareTag("Face"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveFace>().meshRebuilder;

           // materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = false;
           // materialSwap.material = locked;
            currentObj.GetComponent<MoveVertices>().isLocked = true;
            Face currFace = currentObj.GetComponent<Face>();

            foreach(Edge e in meshRebuilder.edgeObjects)
            {
                if(e.id == currFace.edge1 || e.id == currFace.edge2 || e.id == currFace.edge3)
                {
                    e.GetComponent<XRGrabInteractable>().enabled = false;
                    materialSwap = e.GetComponent<MeshRenderer>();
                    materialSwap.material = lockedEdge;
                    e.locked = true;
                    e.GetComponent<MoveEdge>().isLocked = true;
                }
            }

            foreach(Vertex v in meshRebuilder.vertexObjects)
            {
                if(v.id == currFace.vert1 || v.id == currFace.vert2 || v.id == currFace.vert3)
                {
                    v.GetComponent<XRGrabInteractable>().enabled = false;
                    materialSwap = v.GetComponent<MeshRenderer>();
                    materialSwap.material = locked;
                    v.GetComponent<MoveVertices>().isLocked = true;
                }
            }

            // if (sendVertexLockEvent)
            // {
            //     // Synchronize the cached vertex lock event to other players by vertex id
            //     VertexLockEvent vertexLockEvent = new VertexLockEvent()
            //     {
            //         id = currFace.id,
            //         meshId = meshRebuilder.id,
            //         isCached = true,
            //         locked = true,
            //         actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            //     };

            //     NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
            // }
        }

       
    }

    // Change material, enbable vertex grab interactable, set boolean
    public void Unlock(GameObject currentObj, bool sendVertexLockEvent = true)
    {
        if(currentObj == null)
            return;
        if(currentObj.CompareTag("Vertex"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveVertices>().meshRebuilder;

            materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = true;
            materialSwap.material = unselected;
            currentObj.GetComponent<MoveVertices>().isLocked = false;
            Vertex currVert = currentObj.GetComponent<Vertex>();

            
            foreach(Edge e in meshRebuilder.edgeObjects)
            {
                if(e.vert1 == currVert.id || e.vert2 == currVert.id)
                {
                    e.GetComponent<XRGrabInteractable>().enabled = true;
                    materialSwap = e.GetComponent<MeshRenderer>();
                    materialSwap.material = unselected;
                    e.locked = false;
                    e.GetComponent<MoveEdge>().isLocked = false;
                }

            }

            foreach(Face f in meshRebuilder.faceObjects)
            {
                if(f.vert1 == currVert.id || f.vert2 == currVert.id || f.vert3 == currVert.id)
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
                    id = currVert.id,
                    meshId = meshRebuilder.id,
                    isCached = true,
                    locked = false,
                    actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
                };

                NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
            }
        }

        if(currentObj.CompareTag("Edge"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveEdge>().meshRebuilder;

            materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = true;
            materialSwap.material = unselected;
            currentObj.GetComponent<MoveVertices>().isLocked = false;
            Edge currEdge = currentObj.GetComponent<Edge>();

            
            foreach(Vertex v in meshRebuilder.vertexObjects)
            {
                if(v.id == currEdge.vert1 || v.id == currEdge.vert2)
                {
                    v.GetComponent<XRGrabInteractable>().enabled = true;
                    materialSwap = v.GetComponent<MeshRenderer>();
                    materialSwap.material = unselected;
                    v.GetComponent<MoveVertices>().isLocked = false;
                }
            }

            foreach(Face f in meshRebuilder.faceObjects)
            {
                if(f.edge1 == currEdge.id || f.edge2 == currEdge.id  || f.edge3 == currEdge.id)
                {
                    f.GetComponent<XRGrabInteractable>().enabled = true;
                    f.GetComponent<MoveFace>().isLocked = false;
                    f.locked = false;
                }
            }

            // // Only send the event if specified by the bool parameter "sendFaceExtrudeEvent"
            // if (sendVertexLockEvent)
            // {
            //     // Synchronize the cached vertex lock event to other players by vertex id
            //     VertexLockEvent vertexLockEvent = new VertexLockEvent()
            //     {
            //         id = currEdge.id,
            //         meshId = meshRebuilder.id,
            //         isCached = true,
            //         locked = false,
            //         actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            //     };

            //     NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
            // }
        }

        if(currentObj.CompareTag("Face"))
        {
            MeshRebuilder meshRebuilder = currentObj.GetComponent<MoveFace>().meshRebuilder;

            //materialSwap = currentObj.GetComponent<MeshRenderer>();
            currentObj.GetComponent<XRGrabInteractable>().enabled = true;
           // materialSwap.material = unselected;
            currentObj.GetComponent<MoveVertices>().isLocked = false;
            Face currFace = currentObj.GetComponent<Face>();

            
            foreach(Edge e in meshRebuilder.edgeObjects)
            {
                if(currFace.edge1 == e.id || currFace.edge2 == e.id  || currFace.edge3 == e.id)
                {
                    e.GetComponent<XRGrabInteractable>().enabled = true;
                    materialSwap = e.GetComponent<MeshRenderer>();
                    materialSwap.material = unselected;
                    e.locked = false;
                    e.GetComponent<MoveEdge>().isLocked = false;
                }

            }

            foreach(Vertex v in meshRebuilder.vertexObjects)
            {
                if(v.id == currFace.vert1 || v.id == currFace.vert2 || v.id == currFace.vert3)
                {
                    v.GetComponent<XRGrabInteractable>().enabled = true;
                    materialSwap = v.GetComponent<MeshRenderer>();
                    materialSwap.material = unselected;
                    v.GetComponent<MoveVertices>().isLocked = false;
                }
            }

            // // Only send the event if specified by the bool parameter "sendFaceExtrudeEvent"
            // if (sendVertexLockEvent)
            // {
            //     // Synchronize the cached vertex lock event to other players by vertex id
            //     VertexLockEvent vertexLockEvent = new VertexLockEvent()
            //     {
            //         id = currFace.id,
            //         meshId = meshRebuilder.id,
            //         isCached = true,
            //         locked = false,
            //         actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            //     };

            //     NetworkMeshManager.instance.SynchronizeMeshVertexLock(vertexLockEvent);
           // }
        }
    }

    // Get vertex info from sphere collision
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vertex") || other.CompareTag("Edge") || other.CompareTag("Face"))
        {
            currentObj = other.gameObject; 
            inRadius = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if(!switchControllers.rayActive)
        {
            inRadius = false;
            currentObj = null;
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
            
            if(ray.hitVertex || ray.hitEdge || ray.hitFace)
            {
                currentObj = ray.hit.transform.gameObject;
                if(primaryButtonPressed)
                    Lock(currentObj);
                if(secondaryButtonPressed)
                    Unlock(currentObj);
            }
            else
            {
                inRadius = false;
                currentObj = null;
            }
        }
    }
}
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
using System.Linq;

public class Extrude : ToolClass
{
    // [SerializeField] Material locked;     // red
    // [SerializeField] Material unselected;   // gray
    // [SerializeField] Material hovered;      // orange

    [SerializeField] SwitchControllers switchControllers;

    [SerializeField] ToolRaycast ray;


    public XRGrabInteractable vertexGrabInteractable;

   // public PulleyLocomotion pulleyLocomotion;
    //public GameObject editingSpace;
    public GameObject currentFace;

    public bool inRadius = false;

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;

    //GameObject model;

   // MeshRenderer materialSwap;

    private bool hover = false;


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
       // holdTime = 0f;
    }

    public override void PrimaryAction()
    {
        if(!inRadius)
            return;

        if(currentFace == null)
            return;

        Face faceObj = currentFace.GetComponent<Face>();
        MoveFace moveFace = faceObj.gameObject.GetComponent<MoveFace>();
        MeshRebuilder meshRebuilder = moveFace.meshRebuilder;
        Mesh mesh = moveFace.mesh;
        extrudeFace(faceObj.id, meshRebuilder, mesh);
    }

    // Change material, disable vertex grab interactable, set boolean
    public void extrudeFace(int faceId, MeshRebuilder meshRebuilder, Mesh mesh, bool sendFaceExtrudeEvent = true)
    {
        Face faceObj = meshRebuilder.faceObjects[faceId];
        Vertex vertex1 = meshRebuilder.vertexObjects[faceObj.vert1];
        Vertex vertex2 = meshRebuilder.vertexObjects[faceObj.vert2];
        Vertex vertex3 = meshRebuilder.vertexObjects[faceObj.vert3];

        // Don't extrude if one of the vertices is locked
        if (vertex1.GetComponent<MoveVertices>().isLocked ||
            vertex2.GetComponent<MoveVertices>().isLocked ||
            vertex3.GetComponent<MoveVertices>().isLocked)
        {
            print("Unlock face vertices before extruding.");
            return;
        }

        // calculating normals is done in mesh rebuilder

        // align new vertices w/ face normal
        Vector3 new1 = vertex1.transform.localPosition + ((faceObj.normal  + vertex1.transform.localPosition *.005f).normalized );
        Vector3 new2 = vertex2.transform.localPosition + ((faceObj.normal  + vertex2.transform.localPosition *.005f).normalized );
        Vector3 new3 = vertex3.transform.localPosition + ((faceObj.normal  + vertex3.transform.localPosition *.005f).normalized );

       
        List<Vector3> vertList = new List<Vector3>(meshRebuilder.vertices);
        Vector3[] vertices = vertList.ToArray();
        vertList.Add(new1);
        vertList.Add(new2);
        vertList.Add(new3);

        int newVert1 = vertList.Count-3;
        int newVert2 = vertList.Count-2;
        int newVert3 = vertList.Count-1;

        // Verticies along a diagonal will be duplicate if the face hasn't been moved (both tris of one of the sides of a cube)
        for(int i = 0; i < vertices.Length; i++)
        {
            if(Vector3.Distance(vertices[i], new1) < .0001 && newVert1 == vertList.Count-3)
                newVert1 = i;
            if(Vector3.Distance(vertices[i], new2) < .0001 && newVert2 == vertList.Count-2)
                newVert2 = i;
            if(Vector3.Distance(vertices[i], new3) < .0001 && newVert3 == vertList.Count-1)
                newVert3 = i;
        }

       List<Vector3> vertUnique = new List<Vector3>(meshRebuilder.vertices);

        // Only add to list if vertex isn't already in
        if(newVert1 == vertList.Count-3)
               vertUnique.Add(new1);
        if(newVert2 == vertList.Count-2)
               vertUnique.Add(new2);
        if(newVert3 == vertList.Count-1)
               vertUnique.Add(new3);

        // If one of the three was a duplicate then the non duplicate index will be out of bounds
        if(newVert1 > vertUnique.Count - 1)
            newVert1 -= newVert1 - vertUnique.Count + 1 ;
        if(newVert2 > vertUnique.Count - 1)
            newVert2 -= newVert2 - vertUnique.Count + 1;
        if(newVert3 > vertUnique.Count - 1)
            newVert3 -= newVert3 - vertUnique.Count + 1;

        vertices = vertUnique.ToArray();

         //print("new indexes " + newVert1 + " " + newVert2 + " " + newVert3);

        // Add new triangles
        // maybe look thru tris for old 123 and then skip
        List<int> triList = new List<int>(meshRebuilder.triangles);


        // new1, old1, new2
        triList.Add(newVert1);
        triList.Add(faceObj.vert1);
        triList.Add(newVert2);

        // new2, old1, old2
        triList.Add(newVert2);
        triList.Add(faceObj.vert1);
        triList.Add(faceObj.vert2);
        
        //new3, new2, old2
        triList.Add(newVert3);
        triList.Add(newVert2);
        triList.Add(faceObj.vert2);

        //new3, old2, old3
        triList.Add(newVert3);
        triList.Add(faceObj.vert2);
        triList.Add(faceObj.vert3);

        //new3, old3, old1
        triList.Add(newVert3);
        triList.Add(faceObj.vert3);
        triList.Add(faceObj.vert1);

        //new3, old1, new1
        triList.Add(newVert3);
        triList.Add(faceObj.vert1);
        triList.Add(newVert1);

        // new123
        triList.Add(newVert1);
        triList.Add(newVert2);
        triList.Add(newVert3);
        int[] tris = triList.ToArray();

        
        // Update mesh data
        mesh.Clear();
        mesh.vertices = vertices;
        meshRebuilder.vertices = vertices;
        mesh.triangles = tris;
        meshRebuilder.triangles = tris;
        mesh.RecalculateNormals();

        // Update mesh visuals
       // meshRebuilder.RemoveDuplicates();
        meshRebuilder.removeVisuals();
        meshRebuilder.CreateVisuals();

        // Only send the event if specified by the bool parameter "sendFaceExtrudeEvent"
        if (sendFaceExtrudeEvent)
        {
            // Synchronize the cached face extrusion event to other players by face id
            FaceExtrudeEvent faceExtrudeEvent = new FaceExtrudeEvent()
            {
                id = faceObj.id,
                meshId = meshRebuilder.id,
                isCached = true,
                released = true,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            NetworkMeshManager.instance.SynchronizeMeshFaceExtrude(faceExtrudeEvent);
        }
        return;
    }

    // Get face info and 3 vertices
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Face"))
        {
            currentFace = other.gameObject; 
            inRadius = true;
        }
    }

    // Clear data on exiting radius
    public void OnTriggerExit(Collider other)
    {
        if(!switchControllers.rayActive)
        {
            inRadius = false;
            currentFace = null;
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

    // Raycast checking
    void Update()
    {
        if(!isEnabled)
            return;
        if(switchControllers.rayActive)
        {
            if(ray.hitFace)
            {
                //currentVertex = ray.hit.transform.gameObject;
                //vertexGrabInteractable = currentVertex.GetComponent<XRGrabInteractable>();

                currentFace = ray.hit.transform.gameObject;
                Face faceObj = currentFace.GetComponent<Face>();
                MoveFace moveFace = faceObj.gameObject.GetComponent<MoveFace>();
                MeshRebuilder meshRebuilder = moveFace.meshRebuilder;
                Mesh mesh = moveFace.mesh;
                inRadius = true;
                
                if(primaryButtonPressed)
                    extrudeFace(faceObj.id, meshRebuilder, mesh);
            }
            else
            {
                inRadius = false;
                currentFace = null;
            }
        }
    }
}

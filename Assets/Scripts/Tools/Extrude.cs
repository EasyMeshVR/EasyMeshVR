using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Realtime;
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

   // public PulleyLocomotion pulleyLocomotion;
    //public GameObject editingSpace;
    public GameObject currentFace;

    public bool inRadius = false;
    

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;

    //GameObject model;

   // MeshRenderer materialSwap;
    private bool hover = false;

    public GameObject vertex;
    public GameObject edge;

    public GameObject face;


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
        // if (vertex1.GetComponent<MoveVertices>().isLocked ||
        //     vertex2.GetComponent<MoveVertices>().isLocked ||
        //     vertex3.GetComponent<MoveVertices>().isLocked)
        // {
        //     print("Unlock face vertices before extruding.");
        //     return;
        // }

        // calculating normals is done in mesh rebuilder

        // align new vertices w/ face normal
        Vector3 new1 = vertex1.transform.localPosition + ((faceObj.normal  + vertex1.transform.localPosition *.005f).normalized );
        Vector3 new2 = vertex2.transform.localPosition + ((faceObj.normal  + vertex2.transform.localPosition *.005f).normalized );
        Vector3 new3 = vertex3.transform.localPosition + ((faceObj.normal  + vertex3.transform.localPosition *.005f).normalized );

        List<Vector3> newVertices = new List<Vector3>();

       
        List<Vector3> vertList = new List<Vector3>(meshRebuilder.vertices);
        int oldLength = vertList.Count;
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
        {
            vertUnique.Add(new1);
            newVertices.Add(new1);
        }
        if(newVert2 == vertList.Count-2)
        {
            vertUnique.Add(new2);
            newVertices.Add(new2);

        }
        if(newVert3 == vertList.Count-1)
        {
            vertUnique.Add(new3);
            newVertices.Add(new3);
        }

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
        int oldLengthTri = triList.Count;
        List<int> newTriangles = new List<int>();


        // new1, old1, new2
        newTriangles.Add(newVert1);
        newTriangles.Add(faceObj.vert1);
        newTriangles.Add(newVert2);

        // new2, old1, old2
        newTriangles.Add(newVert2);
        newTriangles.Add(faceObj.vert1);
        newTriangles.Add(faceObj.vert2);
        
        //new3, new2, old2
        newTriangles.Add(newVert3);
        newTriangles.Add(newVert2);
        newTriangles.Add(faceObj.vert2);

        //new3, old2, old3
        newTriangles.Add(newVert3);
        newTriangles.Add(faceObj.vert2);
        newTriangles.Add(faceObj.vert3);

        //new3, old3, old1
        newTriangles.Add(newVert3);
        newTriangles.Add(faceObj.vert3);
        newTriangles.Add(faceObj.vert1);

        //new3, old1, new1
        newTriangles.Add(newVert3);
        newTriangles.Add(faceObj.vert1);
        newTriangles.Add(newVert1);

        // new123
        newTriangles.Add(newVert1);
        newTriangles.Add(newVert2);
        newTriangles.Add(newVert3);

        triList.AddRange(newTriangles);
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
      //  meshRebuilder.removeVisuals();
        //meshRebuilder.CreateVisuals();

        CreateVisuals(meshRebuilder, newVertices, newTriangles, oldLength, oldLengthTri);

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

    // Actually create the vertex and edge GameObject interactables
    void CreateVisuals(MeshRebuilder meshRebuilder, List<Vector3> newVertices, List<int> newTriangles, int oldLengthVert, int oldLengthTri)
    {
        Vector3[]vertices = meshRebuilder.vertices;
        int[] triangles = meshRebuilder.triangles;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Create a new vertex from a prefab, make it a child of the mesh and set it's position
            GameObject newVertex = Instantiate(vertex, meshRebuilder.model.transform);
            
            newVertex.transform.localPosition = vertices[i];

            newVertex.name = "Vertex" + i.ToString();

            // Set the id of the Vertex component to be the index in the vertices array
            Vertex vertexObj = newVertex.GetComponent<Vertex>();
            vertexObj.id = i;
            meshRebuilder.vertexObjects.Add(vertexObj);

            if (!ToolManager.instance.grabVertex)
            {
                newVertex.SetActive(false);
            }

            // Save vertices adjacent to the one we're currently looking at (no duplicates)
            HashSet<int> adjacentVertices = new HashSet<int>();

            // Loop through the triangles array and look for the adjacent vertices
            for (int j = 0; j < triangles.Length; j += 3)
            {
                // Triangles are created in triplets
                // Entering "0, 1, 2," in the triangles array would make a triangle

                if (triangles[j] == i) // First index of triplet
                {
                    adjacentVertices.Add(triangles[j + 1]);
                    adjacentVertices.Add(triangles[j + 2]);
                }
                else if (triangles[j + 1] == i) // Second index of triplet
                {
                    adjacentVertices.Add(triangles[j]);
                    adjacentVertices.Add(triangles[j + 2]);
                }
                else if (triangles[j + 2] == i) // Third index of triplet
                {
                    adjacentVertices.Add(triangles[j]);
                    adjacentVertices.Add(triangles[j + 1]);
                }
            }

            // Connect a line from our starting vertex to each adjacent vertex
            foreach (int k in adjacentVertices)
            {
                // Ignore adjacent vertices we've already dealt with
                if (k < i)
                    continue;

                // Same as vertex, create a new edge object and set its parent
                GameObject newEdge = Instantiate(edge, meshRebuilder.model.transform);

                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[i], vertices[k]);
                newEdge.transform.localPosition = (vertices[i] + vertices[k]) / 2;
                newEdge.transform.localScale = new Vector3(newEdge.transform.localScale.x, edgeDistance, newEdge.transform.localScale.z);

                // Orient the edge to look at the vertices
                newEdge.transform.LookAt(newVertex.transform, Vector3.up);
                newEdge.transform.rotation *= Quaternion.Euler(90, 0, 0);

                // Add edge and it's connecting vertices to a dictionary reference for use in other scripts
                Edge edgeComponent = newEdge.GetComponent<Edge>();
                edgeComponent.id = meshRebuilder.edgeObjects.Count();
                edgeComponent.vert1 = i;
                edgeComponent.vert2 = k;
                meshRebuilder.edgeObjects.Add(edgeComponent);

                if (!ToolManager.instance.grabEdge)
                {
                    newEdge.SetActive(false);
                }

                if(i < oldLengthVert && k < oldLengthVert)
                {
                    GameObject.Destroy(newEdge);
                    meshRebuilder.edgeObjects.Remove(edgeComponent);
                }
            }

            if( i < oldLengthVert)
            {
                GameObject.Destroy(newVertex);
                meshRebuilder.vertexObjects.Remove(vertexObj);
            }
        }
        

        // Triangle handles
        for(int i = oldLengthTri; i < triangles.Length; i+=3)
        {
            GameObject newFace = Instantiate(face, meshRebuilder.model.transform);
            // Add face to list and get vertices
            Face faceComponent = newFace.GetComponent<Face>();
            faceComponent.id = meshRebuilder.faceObjects.Count();
            faceComponent.vert1 = triangles[i];
            faceComponent.vert2 = triangles[i+1];
            faceComponent.vert3 = triangles[i+2];

            faceComponent.vertObj1 = meshRebuilder.vertexObjects[faceComponent.vert1];
            faceComponent.vertObj2 = meshRebuilder.vertexObjects[faceComponent.vert2];
            faceComponent.vertObj3 = meshRebuilder.vertexObjects[faceComponent.vert3];

            // Store face normal
            Vector3 e1 = vertices[faceComponent.vert2] - vertices[faceComponent.vert1];
            Vector3 e2 = vertices[faceComponent.vert3] - vertices[faceComponent.vert2];
            faceComponent.normal = Vector3.Normalize(Vector3.Cross(e1,e2));

            // Place face object in center of triangle
            float totalX = vertices[faceComponent.vert1].x + vertices[faceComponent.vert2].x + vertices[faceComponent.vert3].x;
            float totalY = vertices[faceComponent.vert1].y + vertices[faceComponent.vert2].y + vertices[faceComponent.vert3].y;
            float totalZ = vertices[faceComponent.vert1].z + vertices[faceComponent.vert2].z + vertices[faceComponent.vert3].z;

            // Store edge
            foreach(Edge edge in meshRebuilder.edgeObjects)
            {
                 if((edge.vert1 == faceComponent.vert1 && edge.vert2 == faceComponent.vert2) || (edge.vert2 == faceComponent.vert1 && edge.vert1 == faceComponent.vert2))
                {
                    faceComponent.edge1 = edge.id;
                    faceComponent.edgeObj1 = meshRebuilder.edgeObjects[edge.id];
                }
                if((edge.vert1 == faceComponent.vert2 && edge.vert2 == faceComponent.vert3) || (edge.vert2 == faceComponent.vert2 && edge.vert1 == faceComponent.vert3))
                {
                    faceComponent.edge2 = edge.id;
                    faceComponent.edgeObj2 = meshRebuilder.edgeObjects[edge.id];

                }
                if((edge.vert1 == faceComponent.vert1 && edge.vert2 == faceComponent.vert3) || (edge.vert2 == faceComponent.vert1 && edge.vert1 == faceComponent.vert3))
                {
                    faceComponent.edge3 = edge.id;
                    faceComponent.edgeObj3 = meshRebuilder.edgeObjects[edge.id];
                }
            }
            newFace.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);

            meshRebuilder.faceObjects.Add(faceComponent);

            if (!ToolManager.instance.grabFace)
            {
                newFace.SetActive(false);
            }
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

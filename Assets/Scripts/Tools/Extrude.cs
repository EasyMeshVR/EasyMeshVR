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
    [SerializeField] SwitchControllers switchControllers;
    [SerializeField] ToolRaycast ray;
    [SerializeField] XRDirectInteractor directInteractor;
    [SerializeField] Material unselected;
    [SerializeField] Material lockedEdge;


    public GameObject currentFace;
    public bool inRadius = false;

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;
    private bool hover = false;

    public GameObject vertex;
    public GameObject edge;
    public GameObject face;
    GameObject newFace;
    XRGrabInteractable newGrab;
    public bool movingNewFace = false;
    public MeshRenderer materialSwap;

    public class ExtrudedObjects
    {
        public List<int> newVertexIds;
        public int newTriangleIndexStart;
        public int newTriangleCount;
    }

    void OnEnable()
    {
        leftSphere = GameObject.Find("LeftRadius").GetComponent<SphereCollider>();
        rightSphere = GameObject.Find("RightRadius").GetComponent<SphereCollider>();
    }

    public override void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

    // Extrude along normal
    public override void PrimaryAction()
    {
        if(!inRadius || movingNewFace)
            return;

        if(currentFace == null)
            return;

        Face faceObj = currentFace.GetComponent<Face>();
        MoveFace moveFace = faceObj.gameObject.GetComponent<MoveFace>();
        MeshRebuilder meshRebuilder = moveFace.meshRebuilder;
        float extrudeDistance = 1f;
        AddExtrudeOpStep(meshRebuilder.id, faceObj.id, extrudeDistance);
    }

    public void AddExtrudeOpStep(int meshId, int faceId, float extrudeDistance, bool sendFaceExtrudeEvent = true)
    {
        Step step = new Step();
        ExtrudeOp op = new ExtrudeOp(meshId, faceId, extrudeDistance, sendFaceExtrudeEvent);
        step.AddOp(op);
        StepExecutor.instance.AddStep(step);
    }

    // Extrude and move on first press, stop moving on second press
    public override void SecondaryAction()
    {
        if(movingNewFace)
        {
            //print("test stop interaction");
            directInteractor.EndManualInteraction();
            movingNewFace = false;
            return;
        }
       
        if(currentFace == null)
            return;

        Face faceObj = currentFace.GetComponent<Face>();
        MoveFace moveFace = faceObj.gameObject.GetComponent<MoveFace>();
        MeshRebuilder meshRebuilder = moveFace.meshRebuilder;
        Mesh mesh = moveFace.mesh;
        float extrudeDistance = 5f;

        if (inRadius && !movingNewFace)
        {
            AddExtrudeOpStep(meshRebuilder.id, faceObj.id, extrudeDistance);
            moveNewFace(meshRebuilder);
            return;
        }
    }

    public override void triggerAction()
    {
        if(movingNewFace)
        {
            //print("test stop interaction");
            directInteractor.EndManualInteraction();
            movingNewFace = false;
            return;
        }

    }

    // Extrude face along normal by distance value set by either button
    public ExtrudedObjects extrudeFace(int faceId, MeshRebuilder meshRebuilder, Mesh mesh, float extrudeDistance, bool sendFaceExtrudeEvent = true)
    {
        Face faceObj = meshRebuilder.faceObjects[faceId];
        Vertex vertex1 = meshRebuilder.vertexObjects[faceObj.vert1];
        Vertex vertex2 = meshRebuilder.vertexObjects[faceObj.vert2];
        Vertex vertex3 = meshRebuilder.vertexObjects[faceObj.vert3];

        // Re-calculate the normal of the face since the positon of the vertices may have changed
        Vector3 e1 = meshRebuilder.vertices[faceObj.vert2] - meshRebuilder.vertices[faceObj.vert1];
        Vector3 e2 = meshRebuilder.vertices[faceObj.vert3] - meshRebuilder.vertices[faceObj.vert2];
        faceObj.normal = Vector3.Normalize(Vector3.Cross(e1, e2));

        // align new vertices w/ face normal
        Vector3 new1 = vertex1.transform.localPosition + ((faceObj.normal  + vertex1.transform.localPosition *.005f).normalized ) / extrudeDistance;
        Vector3 new2 = vertex2.transform.localPosition + ((faceObj.normal  + vertex2.transform.localPosition *.005f).normalized ) / extrudeDistance;
        Vector3 new3 = vertex3.transform.localPosition + ((faceObj.normal  + vertex3.transform.localPosition *.005f).normalized ) / extrudeDistance;

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
       List<int> newVertexIds = new List<int>();

        // Only add to list if vertex isn't already in
        if(newVert1 == vertList.Count-3)
        {
            vertUnique.Add(new1);
            newVertices.Add(new1);
            newVertexIds.Add(newVert1);
        }
        if(newVert2 == vertList.Count-2)
        {
            vertUnique.Add(new2);
            newVertices.Add(new2);
            newVertexIds.Add(newVert2);
        }
        if(newVert3 == vertList.Count-1)
        {
            vertUnique.Add(new3);
            newVertices.Add(new3);
            newVertexIds.Add(newVert3);
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
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
                extrudeDistance = extrudeDistance
            };

            NetworkMeshManager.instance.SynchronizeMeshFaceExtrude(faceExtrudeEvent);
        }

        connectOldVerts(meshRebuilder, vertex1, vertex2, vertex3);

        vertex1.connectedEdges = vertex1.connectedEdges.Distinct().ToList();
        vertex1.connectedFaces = vertex1.connectedFaces.Distinct().ToList();

        vertex2.connectedEdges = vertex2.connectedEdges.Distinct().ToList();
        vertex2.connectedFaces = vertex2.connectedFaces.Distinct().ToList(); 

        vertex3.connectedEdges = vertex3.connectedEdges.Distinct().ToList();
        vertex3.connectedFaces = vertex3.connectedFaces.Distinct().ToList();

        // Lock new edges and triangles connected to locked vertices
        if(vertex1.GetComponent<MoveVertices>().isLocked)
            LockNewVisuals(meshRebuilder, vertex1.id);

        if(vertex2.GetComponent<MoveVertices>().isLocked)
            LockNewVisuals(meshRebuilder, vertex2.id);

        if(vertex3.GetComponent<MoveVertices>().isLocked)
            LockNewVisuals(meshRebuilder, vertex3.id);
        // Return the list of new vertexIds that were generated for this extruded face
        // as well as the amount of triangles generated and the starting index of the
        // new triangles in the array. (Used in ExtrudeOp to undo the mesh triangles/verts extrusion)
        return new ExtrudedObjects
        {
            newVertexIds = newVertexIds,
            newTriangleIndexStart = oldLengthTri,
            newTriangleCount = newTriangles.Count
        };
    }

    // Move the new face when extruding with the secondary button
    public void moveNewFace(MeshRebuilder meshRebuilder)
    {
        movingNewFace = true;
         // Move the new Face on extrusion
        newFace = meshRebuilder.faceObjects[meshRebuilder.faceObjects.Count-1].gameObject;
        newGrab = newFace.GetComponent<MoveFace>().grabInteractable;

        // change material of current face's vertices
        Face current = currentFace.GetComponent<Face>();
        if(!current.vertObj1.GetComponent<MoveVertices>().isLocked)
        {
            materialSwap = current.vertObj1.GetComponent<MeshRenderer>();
            materialSwap.material = unselected;
        }
        if(!current.vertObj2.GetComponent<MoveVertices>().isLocked)
        {
            materialSwap = current.vertObj2.GetComponent<MeshRenderer>();
            materialSwap.material = unselected;
        }
        if(!current.vertObj3.GetComponent<MoveVertices>().isLocked)
        {
            materialSwap = current.vertObj3.GetComponent<MeshRenderer>();
            materialSwap.material = unselected;
        }

        directInteractor.StartManualInteraction(newGrab);
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

    void connectOldVerts(MeshRebuilder meshRebuilder, Vertex old1, Vertex old2, Vertex old3)
    {
        for(int i = 0; i < meshRebuilder.faceObjects.Count; i++)
        {
            Face currentFace = meshRebuilder.faceObjects[i];
            if(currentFace.vert1 == old1.id || currentFace.vert2 == old1.id || currentFace.vert3 == old1.id)
            {
                old1.connectedFaces.Add(currentFace);
                if(currentFace.edgeObj1.vert1 == old1.id || currentFace.edgeObj1.vert2 == old1.id)
                    old1.connectedEdges.Add(currentFace.edgeObj1);
                if(currentFace.edgeObj2.vert1 == old1.id || currentFace.edgeObj2.vert2 == old1.id)
                    old1.connectedEdges.Add(currentFace.edgeObj2);
                if(currentFace.edgeObj3.vert1 == old1.id || currentFace.edgeObj3.vert2 == old1.id)
                    old1.connectedEdges.Add(currentFace.edgeObj3);
            }

            if(currentFace.vert1 == old2.id || currentFace.vert2 == old2.id || currentFace.vert3 == old2.id)
            {
                old2.connectedFaces.Add(currentFace);
                if(currentFace.edgeObj1.vert1 == old2.id || currentFace.edgeObj1.vert2 == old2.id)
                    old2.connectedEdges.Add(currentFace.edgeObj1);
                if(currentFace.edgeObj2.vert1 == old2.id || currentFace.edgeObj2.vert2 == old2.id)
                    old2.connectedEdges.Add(currentFace.edgeObj2);
                if(currentFace.edgeObj3.vert1 == old2.id || currentFace.edgeObj3.vert2 == old2.id)
                    old2.connectedEdges.Add(currentFace.edgeObj3);
            }

            if(currentFace.vert1 == old3.id || currentFace.vert2 == old3.id || currentFace.vert3 == old3.id)
            {
                old1.connectedFaces.Add(currentFace);
                if(currentFace.edgeObj1.vert1 == old3.id || currentFace.edgeObj1.vert2 == old3.id)
                    old3.connectedEdges.Add(currentFace.edgeObj1);
                if(currentFace.edgeObj2.vert1 == old3.id || currentFace.edgeObj2.vert2 == old3.id)
                    old3.connectedEdges.Add(currentFace.edgeObj2);
                if(currentFace.edgeObj3.vert1 == old3.id || currentFace.edgeObj3.vert2 == old3.id)
                    old3.connectedEdges.Add(currentFace.edgeObj3);
            }
        }
    }

    // Actually create the vertex and edge GameObject interactables
    void CreateVisuals(MeshRebuilder meshRebuilder, List<Vector3> newVertices, List<int> newTriangles, int oldLengthVert, int oldLengthTri)
    {
        // int edgeCount = 0;
        // int faceCount = 0;
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
            vertexObj.thisVertex = newVertex;
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
                newEdge.name = "Edge" + (meshRebuilder.edgeObjects.Count).ToString();
            
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
                edgeComponent.thisEdge = newEdge;
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

            // Add Edge id to Vertex component (used in Merge tool)
            foreach (Edge edge in meshRebuilder.edgeObjects)
            {
                if (edge.vert1 == i || edge.vert2 == i)
                    vertexObj.connectedEdges.Add(edge);
            }

            // edgeCount--;

            if ( i < oldLengthVert)
            {
                GameObject.Destroy(newVertex);
                meshRebuilder.vertexObjects.Remove(vertexObj);
            }
        }
        

        // Triangle handles
        for(int i = oldLengthTri; i < triangles.Length; i+=3)
        {
            GameObject newFace = Instantiate(face, meshRebuilder.model.transform);
            newFace.name = "Face" + (meshRebuilder.faceObjects.Count).ToString();

            // Add face to list and get vertices
            Face currentonent = newFace.GetComponent<Face>();
            currentonent.id = meshRebuilder.faceObjects.Count();
            currentonent.vert1 = triangles[i];
            currentonent.vert2 = triangles[i+1];
            currentonent.vert3 = triangles[i+2];

            currentonent.vertObj1 = meshRebuilder.vertexObjects[currentonent.vert1];
            currentonent.vertObj2 = meshRebuilder.vertexObjects[currentonent.vert2];
            currentonent.vertObj3 = meshRebuilder.vertexObjects[currentonent.vert3];

            // Store face normal
            Vector3 e1 = vertices[currentonent.vert2] - vertices[currentonent.vert1];
            Vector3 e2 = vertices[currentonent.vert3] - vertices[currentonent.vert2];
            currentonent.normal = Vector3.Normalize(Vector3.Cross(e1,e2));

            // Place face object in center of triangle
            float totalX = vertices[currentonent.vert1].x + vertices[currentonent.vert2].x + vertices[currentonent.vert3].x;
            float totalY = vertices[currentonent.vert1].y + vertices[currentonent.vert2].y + vertices[currentonent.vert3].y;
            float totalZ = vertices[currentonent.vert1].z + vertices[currentonent.vert2].z + vertices[currentonent.vert3].z;

            // Place faceComponent in Vertex object list (Vertex.cs, used in Merge.cs)
            foreach (Vertex vertex in meshRebuilder.vertexObjects)
            {
                if (vertex.id == currentonent.vert1 || vertex.id == currentonent.vert2 || vertex.id == currentonent.vert3)
                    vertex.connectedFaces.Add(currentonent);
            }

            // Store edge
            foreach (Edge edge in meshRebuilder.edgeObjects)
            {
                 if((edge.vert1 == currentonent.vert1 && edge.vert2 == currentonent.vert2) || (edge.vert2 == currentonent.vert1 && edge.vert1 == currentonent.vert2))
                {
                    currentonent.edge1 = edge.id;
                    currentonent.edgeObj1 = meshRebuilder.edgeObjects[edge.id];
                }
                if((edge.vert1 == currentonent.vert2 && edge.vert2 == currentonent.vert3) || (edge.vert2 == currentonent.vert2 && edge.vert1 == currentonent.vert3))
                {
                    currentonent.edge2 = edge.id;
                    currentonent.edgeObj2 = meshRebuilder.edgeObjects[edge.id];

                }
                if((edge.vert1 == currentonent.vert1 && edge.vert2 == currentonent.vert3) || (edge.vert2 == currentonent.vert1 && edge.vert1 == currentonent.vert3))
                {
                    currentonent.edge3 = edge.id;
                    currentonent.edgeObj3 = meshRebuilder.edgeObjects[edge.id];
                }
            }
            newFace.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);

            currentonent.thisFace = newFace;
            meshRebuilder.faceObjects.Add(currentonent);

            if (!ToolManager.instance.grabFace)
            {
                newFace.SetActive(false);
            }
        }
    }

    // Lock new visuals if any of the old vertices are locked
    void LockNewVisuals(MeshRebuilder meshRebuilder, int lockedVertex)
    {
        Vertex currentVertex = meshRebuilder.vertexObjects[lockedVertex];
        foreach(Edge e in currentVertex.connectedEdges)
        {
            e.GetComponent<XRGrabInteractable>().enabled = false;
            materialSwap = e.GetComponent<MeshRenderer>();
            materialSwap.material = lockedEdge;
            e.locked = true;
            e.GetComponent<MoveEdge>().isLocked = true;     
        }

        foreach(Face f in currentVertex.connectedFaces)
        {
                f.GetComponent<XRGrabInteractable>().enabled = false;
                f.GetComponent<MoveFace>().isLocked = true;
                f.locked = true;
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
                    extrudeFace(faceObj.id, meshRebuilder, mesh, 1f);
            }
            else
            {
                inRadius = false;
                currentFace = null;
            }
        }
    }
}

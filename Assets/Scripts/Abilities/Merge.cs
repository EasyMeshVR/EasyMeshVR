using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public class Merge : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;

    [SerializeField] Material merge;        // yellow
    [SerializeField] Material unselected;   // gray

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    // Mesh updating
    public GameObject model;
    Mesh mesh;
    MeshRebuilder meshRebuilder;
    static Vector3[] vertices;
    static int[] triangles;
    static List<int> triangleReferences = new List<int>();
    static List<int> lastVertTriReferences = new List<int>();
    Vector3[] timelineVertices;
    int[] timelineTriangles;

    // Vertex lookup
    Vertex mergeVertex;
    static Vertex deleterVertex;
    static Vertex takeoverVertex;
    static Vertex relocaterVertex;
    static int vertex1;
    static int vertex2;
    static int lastIndex;
    static bool connection;

    MeshRenderer materialSwap;
    static int triggerCheck = 1;
    static int triggerCount = 0;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get MeshFilter to steal triangles
        meshRebuilder = transform.parent.GetComponent<MeshRebuilder>();
        model = meshRebuilder.model;
        mesh = model.GetComponent<MeshFilter>().mesh;

        // Stealing data (like the government)
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        
        // MeshFilter references for undo/redo
        timelineVertices = meshRebuilder.vertices;
        timelineTriangles = meshRebuilder.triangles;

        for (int i = 0; i < vertices.Length; i++)
            Debug.Log("starting vertices[" + i + "] = " + vertices[i]);

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("starting triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);

        // This vertex (the one we're holding atm)
        // If we drag this vertex on top of another, we merge the two, and we delete the one in our hand
        mergeVertex = GetComponent<Vertex>();

        // Used to check if we're currently holding a vertex
        editingSpace = meshRebuilder.editingSpace;
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        // You know what this is
        materialSwap = GetComponent<MeshRenderer>();
    }

    // Save the data we need for the vertex we're merging (this is the one of the two we're deleting)
    private void getDeleterData()
    {
        // Reset needed data
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Grab last index of the vertices array
        // If our deleter vertex is the last index, we don't care about updating the vertex IDs
        if (vertex1 == vertices.Length - 1)
            lastIndex = -1;
        else
            lastIndex = vertices.Length - 1;

        // Get references to all triangles the vertex is a part of (all adjacent vertices)
        triangleReferences = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Save the starting vertex of the triangle triplet to access it faster later
            if (triangles[i] == vertex1 || triangles[i + 1] == vertex1 || triangles[i + 2] == vertex1)
                triangleReferences.Add(i);
        }
    }

    // Make the second vertex inherit all the data of the first (this is the one of the two we're keeping)
    private void mergeWithTakeover()
    {
        List<Vector3> verticesList = new List<Vector3>();
        List<int> trianglesList = triangles.ToList();

        // Update and remove triangles relative to vertex1
        trianglesList = UpdateTriangles(trianglesList, triangleReferences, vertex1, vertex2);

        // Get references to all triangles the last vertex is a part of (all adjacent vertices)
        if (lastIndex != -1)
        {
            lastVertTriReferences = new List<int>();

            for (int i = 0; i < trianglesList.Count; i += 3)
            {
                // Save the starting vertex of the triangle triplet for the last vertex in the array (for ID updates later)
                if (trianglesList[i] == lastIndex || trianglesList[i + 1] == lastIndex || trianglesList[i + 2] == lastIndex)
                    lastVertTriReferences.Add(i);
            }
        }

        // Used for finding duplicate edges and remapping them to faces
        List<Edge> edgeDupes = new List<Edge>();
        List<Edge> reface = new List<Edge>();

        // Remove mesh object components
        reface = RemoveEdges(edgeDupes, reface);
        RemoveFaces(edgeDupes, reface);
        RelocateObjects();
        verticesList = RemoveVertices(verticesList, trianglesList);

        // Updated data
        vertices = verticesList.ToArray();
        triangles = trianglesList.ToArray();
    }

    // Delete edge that connects the two vertices and all overlapping edges
    private List<Edge> RemoveEdges(List<Edge> edgeDupes, List<Edge> reface)
    {
        // Used for renaming and re-id'ing the edges
        int edgeCountOld = meshRebuilder.edgeObjects.Count;
        List<int> edgesToReID = new List<int>();

        // All edges that were connected to vertex 1, connect to vertex 2
        foreach (Edge reconnect in deleterVertex.connectedEdges)
        {
            // Delete the edge that connects the two vertices
            if (reconnect.vert1 == vertex2 || reconnect.vert2 == vertex2)
            {
                // For moving edges through the edgeObjects list when we're done deleting them
                if (reconnect.id != edgeCountOld - 1)
                    edgesToReID.Add(reconnect.id);

                takeoverVertex.connectedEdges.Remove(reconnect);
                meshRebuilder.edgeObjects.Remove(reconnect);
                Destroy(reconnect.thisEdge);
            }
            else
            {
                // Update vert1 or vert2 ids in the Edge.cs script to reference the takeover
                if (reconnect.vert1 == vertex1)
                    reconnect.vert1 = vertex2;
                else
                    reconnect.vert2 = vertex2;

                // Add the new edge to the takeover vertex
                takeoverVertex.connectedEdges.Add(reconnect);
            }
        }

        // If data from vertex 1 goes to vertex 2, there should be two copies of an edge with the same vertex ids
        // Look through all edges connected to vertex 2, if two share the same vertex ids, delete one of them
        foreach (Edge duplicate1 in deleterVertex.connectedEdges)
        {
            if (duplicate1 == null)
                continue;

            // Compare vertex ids between deleter and takeover to find duplicates to get rid of
            foreach (Edge duplicate2 in takeoverVertex.connectedEdges)
            {
                // Same edge
                if (duplicate1.id == duplicate2.id)
                    continue;

                // Cross vertex id check
                if ((duplicate1.vert1 == duplicate2.vert1 && duplicate1.vert2 == duplicate2.vert2) || (duplicate1.vert1 == duplicate2.vert2 && duplicate1.vert2 == duplicate2.vert1))
                {
                    // Need lists to use indexing, don't want duplicates

                    // Deleting
                    if (!edgeDupes.Contains(duplicate1))
                        edgeDupes.Add(duplicate1);

                    // Keeping
                    if (!reface.Contains(duplicate2))
                        reface.Add(duplicate2);
                    
                    // For moving edges through the edgeObjects list when we're done deleting them
                    if (!edgesToReID.Contains(duplicate1.id))
                        edgesToReID.Add(duplicate1.id);
                }
            }
        }

        // Actually delete the edges from the takeover
        foreach (Edge confirmed in edgeDupes)
        {
            takeoverVertex.connectedEdges.Remove(confirmed);
            meshRebuilder.edgeObjects.Remove(confirmed);
            Destroy(confirmed.thisEdge);
        }

        int edgeCountDiff = edgeCountOld - meshRebuilder.edgeObjects.Count;

        Debug.Log("-----------------");
        Debug.Log("edgeCountDiff = " + edgeCountDiff);
        Debug.Log("meshRebuilder.edgeObjects.Count = " + meshRebuilder.edgeObjects.Count);
        Debug.Log("-----------------");
        Debug.Log("edgesToReID = " + edgesToReID.Count);
        foreach (int i in edgesToReID)
            Debug.Log("edges: " + i);
        Debug.Log("-----------------");
        foreach (Edge edge in edgeDupes)
            Debug.Log("edgeDupes: " + edge.id);
        foreach (Edge edge in reface)
            Debug.Log("refaces: " + edge.id);
        Debug.Log("-----------------");

        ReIDEdges(edgeCountDiff, edgesToReID);

        return reface;
    }

    private void ReIDEdges(int edgeCountDiff, List<int> edgesToReID)
    {
        int idCount = 0;

        for (int i = meshRebuilder.edgeObjects.Count - 1; i >= 0; i--)
        {
            if (idCount > edgesToReID.Count - 1)
                break;

            if (meshRebuilder.edgeObjects[i].id == meshRebuilder.edgeObjects.Count - 1)
            {
                idCount++;
            }
            else
            {
                meshRebuilder.edgeObjects[i].id = edgesToReID[idCount];
                meshRebuilder.edgeObjects[i].name = "Edge" + (edgesToReID[idCount]).ToString();
                idCount++;
            }
        }
    }

    // Delete faces after merging two vertices
    void RemoveFaces(List<Edge> edgeDupes, List<Edge> reface)
    {
        // Used for renaming and re-id'ing the faces
        int faceCountOld = meshRebuilder.faceObjects.Count;
        List<int> facesToReID = new List<int>();

        // Sift through the deleter vertex faces (vertex1)
        foreach (Face face in deleterVertex.connectedFaces)
        {
            // If any of the face vertices contain both deleter and takeover (vertex1 and 2), delete
            if (face.vert1 == vertex2 || face.vert2 == vertex2 || face.vert3 == vertex2)
            {
                // For moving faces through the faceObjects list when we're done deleting them
                if (face.id != faceCountOld - 1)
                    facesToReID.Add(face.id);

                takeoverVertex.connectedFaces.Remove(face);
                meshRebuilder.faceObjects.Remove(face);
                Destroy(face.thisFace);
            }
            else
            {
                // Update vert1 or vert2 ids in the Face.cs script to reference the takeover
                if (face.vert1 == vertex1)
                {
                    face.vert1 = vertex2;
                    face.vertObj1 = takeoverVertex;
                }
                else if (face.vert2 == vertex1)
                {
                    face.vert2 = vertex2;
                    face.vertObj2 = takeoverVertex;
                }
                else if (face.vert3 == vertex1)
                {
                    face.vert3 = vertex2;
                    face.vertObj3 = takeoverVertex;
                }

                // Add the new face to the takeover vertex
                takeoverVertex.connectedFaces.Add(face);
            }

            // Deleting edges causes Faces to lose edge data
            // Compare the edge ids from edgeDupes list and replace it with the id from reface list
            for (int i = 0; i < reface.Count; i++)
            {
                if (face.edge1 == edgeDupes[i].id)
                {
                    face.edgeObj1 = reface[i].thisEdge.GetComponent<Edge>();
                    face.edge1 = face.edgeObj1.id;
                }
                else if (face.edge2 == edgeDupes[i].id)
                {
                    face.edgeObj2 = reface[i].thisEdge.GetComponent<Edge>();
                    face.edge2 = face.edgeObj2.id;
                }
                else if (face.edge3 == edgeDupes[i].id)
                {
                    face.edgeObj3 = reface[i].thisEdge.GetComponent<Edge>();
                    face.edge3 = face.edgeObj3.id;
                }
            }

            face.edge1 = face.edgeObj1.id;
            face.edge2 = face.edgeObj2.id;
            face.edge3 = face.edgeObj3.id;
        }

        int faceCountDiff = faceCountOld - meshRebuilder.faceObjects.Count;
        ReIDFaces(faceCountDiff, facesToReID);
    }

    void ReIDFaces(int faceCountDiff, List<int> facesToReID)
    {
        int idCount = 0;

        for (int i = meshRebuilder.faceObjects.Count - 1; i >= 0; i--)
        {
            if (idCount > facesToReID.Count - 1)
                break;

            if (meshRebuilder.faceObjects[i].id == meshRebuilder.faceObjects.Count - 1)
            {
                idCount++;
            }
            else
            {
                meshRebuilder.faceObjects[i].id = facesToReID[idCount];
                meshRebuilder.faceObjects[i].name = "Face" + (facesToReID[idCount]).ToString();
                idCount++;
            }
        }
    }

    void RelocateObjects()
    {
        List<Edge> sortedEdges = meshRebuilder.edgeObjects.OrderBy(e => e.id).ToList();
        List<Face> sortedFaces = meshRebuilder.faceObjects.OrderBy(f => f.id).ToList();

        meshRebuilder.edgeObjects = sortedEdges;
        meshRebuilder.faceObjects = sortedFaces;

        // Edge move = meshRebuilder.edgeObjects[i];
        // meshRebuilder.edgeObjects.Remove(move);
        // meshRebuilder.edgeObjects.Insert(move.id, move);

        // Face move = meshRebuilder.faceObjects[i];
        // meshRebuilder.faceObjects.Remove(move);
        // meshRebuilder.faceObjects.Insert(move.id, move);
    }

    // Remove vertex from the mesh filter, special case for removing vertices in the middle of the array
    private List<Vector3> RemoveVertices(List<Vector3> verticesList, List<int> trianglesList)
    {
        // Remove vertex1 from the vertices array and meshRebuilder vertexObjects list
        verticesList = vertices.ToList();
        verticesList.RemoveAt(vertex1);
        meshRebuilder.vertexObjects.Remove(deleterVertex);

        // Update the last vertex to have the same IDs as the one we just deleted (to avoid out of bounds errors later)
        // If the new last vertex is our takeover vertex, all this is unncessary
        if (lastIndex != -1)
        {
            // Move vertex in last position to the position of the one we just deleted
            Vector3 lastVertex = verticesList[verticesList.Count - 1];
            verticesList.RemoveAt(verticesList.Count - 1);
            verticesList.Insert(vertex1, lastVertex);

            // Update the triangles for the newly moved last vertex
            trianglesList = UpdateTriangles(trianglesList, lastVertTriReferences, vertices.Length - 1, vertex1);

            // Update vertex name and IDs in Vertex.cs (Vertex.id)
            // Vertex lastVertexGO = GameObject.Find("Vertex" + (vertices.Length - 1)).GetComponent<Vertex>();
            Vertex lastVertexGO = meshRebuilder.vertexObjects[meshRebuilder.vertexObjects.Count - 1];
            meshRebuilder.vertexObjects.RemoveAt(verticesList.Count - 1);
            meshRebuilder.vertexObjects.Insert(vertex1, lastVertexGO);
            lastVertexGO.name = "Vertex" + vertex1.ToString();
            lastVertexGO.id = vertex1;
            relocaterVertex = lastVertexGO;

            // Update Edge.cs (Vertex.connectedEdges -> Edge.vert1 or Edge.vert2)
            foreach (Edge reconnect in lastVertexGO.connectedEdges)
            {
                // Update vert1 or vert2 ids in the Edge.cs script
                if (reconnect.vert1 == vertices.Length - 1)
                    reconnect.vert1 = vertex1;
                else
                    reconnect.vert2 = vertex1;
            }

            // Update the Face vertIDs and vertOBJs to correspond to the last vertex that was just moved
            foreach (Face remap in lastVertexGO.connectedFaces)
            {
                // Last vertex goes to deleter vertex position
                if (remap.vert1 == vertices.Length - 1)
                {
                    remap.vert1 = vertex1;
                    remap.vertObj1 = lastVertexGO;
                }
                else if (remap.vert2 == vertices.Length - 1)
                {
                    remap.vert2 = vertex1;
                    remap.vertObj2 = lastVertexGO;
                }
                else if (remap.vert3 == vertices.Length - 1)
                {
                    remap.vert3 = vertex1;
                    remap.vertObj3 = lastVertexGO;
                }
            }
        }

        return verticesList;
    }

    // Removes triangles with both deleter and takeover in it, replaces deleter ID with takeover ID otherwise
    // Go backwards through the references so we don't accidentally update while we remove
    private List<int> UpdateTriangles(List<int> trianglesList, List<int> triangleReferences, int deleter, int takeover)
    {
        for (int i = triangleReferences.Count - 1; i >= 0; i--)
        {
            if (trianglesList[triangleReferences[i]] == deleter)
            {
                if (trianglesList[triangleReferences[i] + 1] == takeover || trianglesList[triangleReferences[i] + 2] == takeover)
                    trianglesList.RemoveRange(triangleReferences[i], 3);
                else
                    trianglesList[triangleReferences[i]] = takeover;
            }
            else if (trianglesList[triangleReferences[i] + 1] == deleter)
            {
                if (trianglesList[triangleReferences[i]] == takeover || trianglesList[triangleReferences[i] + 2] == takeover)
                    trianglesList.RemoveRange(triangleReferences[i], 3);
                else
                    trianglesList[triangleReferences[i] + 1] = takeover;
            }
            else if (trianglesList[triangleReferences[i] + 2] == deleter)
            {
                if (trianglesList[triangleReferences[i]] == takeover || trianglesList[triangleReferences[i] + 1] == takeover)
                    trianglesList.RemoveRange(triangleReferences[i], 3);
                else
                    trianglesList[triangleReferences[i] + 2] = takeover;
            }
        }

        return trianglesList;
    }

    // Update MeshFilter and re-draw in-game visuals
    public void UpdateMesh(int index)
    {
        /*
        for (int i = 0; i < vertices.Length; i++)
            Debug.Log("vertices[" + i + "] = " + vertices[i]);

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        */

        // Update actual mesh data
        mesh.Clear();
        mesh.vertices = vertices;
        meshRebuilder.vertices = vertices;
        mesh.triangles = triangles;
        meshRebuilder.triangles = triangles;
        mesh.RecalculateNormals();

        /*
        for (int i = 0; i < mesh.vertices.Length; i++)
            Debug.Log("mesh.vertices[" + i + "] = " + mesh.vertices[i]);

        for (int i = 0; i < mesh.triangles.Length; i += 3)
            Debug.Log("mesh.triangles = " + mesh.triangles[i] + ", " + mesh.triangles[i + 1] + ", " + mesh.triangles[i + 2]);

        for (int i = 0; i < meshRebuilder.vertices.Length; i++)
            Debug.Log("meshRebuilder.vertices[" + i + "] = " + meshRebuilder.vertices[i]);

        for (int i = 0; i < meshRebuilder.triangles.Length; i += 3)
            Debug.Log("meshRebuilder.triangles = " + meshRebuilder.triangles[i] + ", " + meshRebuilder.triangles[i + 1] + ", " + meshRebuilder.triangles[i + 2]);
        */

        // Make sure everthing's in its designated spots
        UpdateVertices(takeoverVertex.thisVertex.transform, takeoverVertex.id);
        UpdateEdges(takeoverVertex);

        UpdateFaces(takeoverVertex);
        if (lastIndex != -1)
            UpdateFaces(relocaterVertex);
    }

    public void UpdateVertices(Transform transform, int index)
    {
        Vector3 editingSpaceScale = editingSpace.transform.localScale;

        // Handle divide by zero error
        if (editingSpaceScale.x == 0 || editingSpaceScale.y == 0 || editingSpaceScale.z == 0)
            return;

        // Calculate inverse scale vector based on editing space scale
        Vector3 inverseScale = new Vector3(
            1.0f / editingSpaceScale.x,
            1.0f / editingSpaceScale.y,
            1.0f / editingSpaceScale.z
        );

        // Translate, Scale, and Rotate the vertex position based on the current transform of the editingSpace object.
        meshRebuilder.vertices[index] =
            Quaternion.Inverse(editingSpace.transform.rotation)
            * Vector3.Scale(inverseScale, transform.position - editingSpace.transform.position);
    }

    void UpdateEdges(Vertex vertex)
    {
        // Reconnect edges to vertices (visually)
        foreach (Edge edge in takeoverVertex.connectedEdges)
        {
            GameObject edgeObject;

            if (edge.thisEdge != null)
                edgeObject = edge.thisEdge;
            else
                continue;

            int vert1 = edge.vert1;
            int vert2 = edge.vert2;

            // Set the edge's position to between the two vertices and scale it appropriately
            float edgeDistance = 0.5f * Vector3.Distance(vertices[vert1], vertices[vert2]);
            edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
            edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

            // Orient the edge to look at the vertices (specifically the one we're currently holding)
            edgeObject.transform.LookAt(transform, Vector3.up);
            edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
        }
    }

    // Put the faces in the center of each triangle ("visually")
    void UpdateFaces(Vertex vertex)
    {
        foreach (Face face in vertex.connectedFaces)
        {
            GameObject faceObject;

            if (face.thisFace != null)
                faceObject = face.thisFace;
            else
                continue;

            int vert1 = face.vert1;
            int vert2 = face.vert2;
            int vert3 = face.vert3;

            float totalX = vertices[vert1].x + vertices[vert2].x + vertices[vert3].x;
            float totalY = vertices[vert1].y + vertices[vert2].y + vertices[vert3].y;
            float totalZ = vertices[vert1].z + vertices[vert2].z + vertices[vert3].z;

            faceObject.transform.localPosition = new Vector3(totalX / 3, totalY / 3, totalZ / 3);
        }
    }

    private void OnTriggerExit(Collider takeover)
    {
        materialSwap.material = unselected;
    }

    // Easiest way to detect a vertex being dragged on top of another was with triggers
    private void OnTriggerStay(Collider takeover)
    {
        // If we collide with something that isn't a vertex, we don't want to continue
        if (takeover.gameObject.tag != "Vertex")
        {
            // Debug.Log("Tag != Vertex");
            return;
        }

        materialSwap.material = merge;

        if (pulleyLocomotion.isMovingVertex == true)
        {
            // Debug.Log("You need to let go of the vertex first.");
            return;
        }

        // Since the trigger is on all vertices, two colliding calls this twice and screws the merge up
        // We also want only the second activation, not the first (it picks the vertex IDs in the wrong order)
        // This is a crude check to make sure that doesn't happen
        if (triggerCheck == 2)
        {
            triggerCheck = 1;
            return;
        }
        triggerCheck = 2;

        meshRebuilder.vertices = mesh.vertices;
        meshRebuilder.triangles = mesh.triangles;

        timelineVertices = meshRebuilder.vertices;
        timelineTriangles = meshRebuilder.triangles;

        // Get Vertex references
        deleterVertex = mergeVertex;
        takeoverVertex = takeover.gameObject.GetComponent<Vertex>();

        // Save IDs
        vertex1 = deleterVertex.id;
        vertex2 = takeoverVertex.id;

        // Check to see if vertex2 shares an edge with vertex1
        foreach (Edge connectCheck in takeoverVertex.connectedEdges)
            if (connectCheck.vert1 == vertex1 || connectCheck.vert2 == vertex1)
                connection = true;

        // If they don't, kick us out of here
        if (!connection)
        {
            Debug.Log("You can't merge vertices that don't share an edge!");
            return;
        }
        connection = false;

        Debug.Log("vertex1 = " + deleterVertex.id);
        Debug.Log("vertex2 = " + takeoverVertex.id);

        getDeleterData();
        mergeWithTakeover();
        UpdateMesh(vertex2);
        Destroy(deleterVertex.thisVertex);

        /*
        Debug.Log("#####################################################################");
        Debug.Log("Vertices");
        foreach (Vertex vertex in meshRebuilder.vertexObjects)
        {
            Debug.Log(vertex.name);
            Debug.Log(vertex.id);

            Debug.Log("+++++++++++++++++");
            foreach (Edge edge in vertex.connectedEdges)
                Debug.Log(edge.id);

            Debug.Log("+++++++++++++++++");
            foreach (Face face in vertex.connectedFaces)
                Debug.Log(face.id);

            Debug.Log("---------------------------------------");
        }

        Debug.Log("#####################################################################");
        Debug.Log("Edges");
        foreach (Edge edge in meshRebuilder.edgeObjects)
        {
            Debug.Log(edge.name);
            Debug.Log(edge.id);
            Debug.Log(edge.vert1);
            Debug.Log(edge.vert2);
            Debug.Log("---------------------------------------");
        }

        Debug.Log("#####################################################################");
        Debug.Log("Faces");
        foreach (Face face in meshRebuilder.faceObjects)
        {
            Debug.Log(face.name);
            Debug.Log(face.id);
            Debug.Log(face.vert1);
            Debug.Log(face.vert2);
            Debug.Log(face.vert3);
            Debug.Log(face.vertObj1.id);
            Debug.Log(face.vertObj2.id);
            Debug.Log(face.vertObj3.id);
            Debug.Log(face.edge1);
            Debug.Log(face.edge2);
            Debug.Log(face.edge3);
            Debug.Log(face.edgeObj1.id);
            Debug.Log(face.edgeObj2.id);
            Debug.Log(face.edgeObj3.id);
            Debug.Log("---------------------------------------");
        }
        Debug.Log("#####################################################################");
        */

        Step step = new Step();
        MeshChange op = new MeshChange(timelineVertices, timelineTriangles);
        step.AddOp(op);
        StepExecutor.instance.AddStep(step);
    }
}

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

        /*
        for (int i = 0; i < vertices.Length; i++)
            Debug.Log("starting vertices[" + i + "] = " + vertices[i]);

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("starting triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        */

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
        // Update and remove triangles relative to vertex1
        List<int> trianglesList = triangles.ToList();
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

        // All edges that were connected to vertex 1, connect to vertex 2
        foreach (Edge reconnect in deleterVertex.connectedEdges)
        {
            // Delete the edge that connects the two vertices
            if (reconnect.vert1 == vertex2 || reconnect.vert2 == vertex2)
            {
                // takeoverVertex.connectedEdges.Remove(reconnect);
                // Destroy(reconnect.thisEdge);
                // meshRebuilder.edgeObjects.Remove(reconnect);
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

        // Sift through the deleter vertex faces (vertex1)
        foreach (Face remove in deleterVertex.connectedFaces)
        {
            // If any of the face vertices contain both deleter and takeover (vertex1 and 2), delete
            if (remove.vert1 == vertex2 || remove.vert2 == vertex2 || remove.vert3 == vertex2)
            {
                takeoverVertex.connectedFaces.Remove(remove);
                Destroy(remove.thisFace);
                meshRebuilder.faceObjects.Remove(remove);
            }
            else
            {
                // Update vert1 or vert2 ids in the Face.cs script to reference the takeover
                if (remove.vert1 == vertex1)
                {
                    remove.vert1 = vertex2;
                    remove.vertObj1 = takeoverVertex;
                }
                else if (remove.vert2 == vertex1)
                {
                    remove.vert2 = vertex2;
                    remove.vertObj2 = takeoverVertex;
                }
                else if (remove.vert3 == vertex1)
                {
                    remove.vert3 = vertex2;
                    remove.vertObj3 = takeoverVertex;
                }

                // Add the new face to the takeover vertex
                takeoverVertex.connectedFaces.Add(remove);
            }
        }

        // Delete edges in the same position (overlapping / duplicate) as one of them is no longer needed
        // > If this is not possible, we can do some kind of dynamic deletion by comparing triangle triplets with edge ids
        // > Or brute force it and delete all edges and remake them based on updated triangles array
        // > NEW IDEA: If data from vertex 1 goes to vertex 2, there should be two copies of an edge with the same vertex ids
        //             Look through all edges connected to vertex 2, if two share the same vertex ids, delete one of them

        // Remove vertex1 from the vertices array and meshRebuilder vertexObjects list
        List<Vector3> verticesList = new List<Vector3>();
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

        vertices = verticesList.ToArray();
        triangles = trianglesList.ToArray();
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
        for (int i = 0; i < vertices.Length; i++)
            Debug.Log("vertices[" + i + "] = " + vertices[i]);

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);

        // Update actual mesh data
        mesh.Clear();
        mesh.vertices = vertices;
        meshRebuilder.vertices = vertices;
        mesh.triangles = triangles;
        meshRebuilder.triangles = triangles;
        mesh.RecalculateNormals();

        for (int i = 0; i < mesh.vertices.Length; i++)
            Debug.Log("mesh.vertices[" + i + "] = " + mesh.vertices[i]);

        for (int i = 0; i < mesh.triangles.Length; i += 3)
            Debug.Log("mesh.triangles = " + mesh.triangles[i] + ", " + mesh.triangles[i + 1] + ", " + mesh.triangles[i + 2]);

        // Reconnect edges to vertices (visually)
        foreach (Edge edge in takeoverVertex.connectedEdges)
        {
            GameObject edgeObject = edge.thisEdge;
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

        // Make sure faces are in the correct spots
        UpdateFaces(takeoverVertex);
        UpdateFaces(relocaterVertex);
    }

    // Put the faces in the center of each triangle ("visually")
    void UpdateFaces(Vertex vertex)
    {
        foreach (Face face in vertex.connectedFaces)
        {
            GameObject faceObject = face.gameObject;
            int vert1 = face.vert1;
            int vert2 = face.vert2;
            int vert3 = face.vert3;

            float totalX = vertices[vert1].x + vertices[vert2].x + vertices[vert3].x;
            float totalY = vertices[vert1].y + vertices[vert2].y + vertices[vert3].y;
            float totalZ = vertices[vert1].z + vertices[vert2].z + vertices[vert3].z;

            faceObject.transform.localPosition = new Vector3(totalX / 3, totalY / 3, totalZ / 3);
        }
    }

    // Easiest way to detect a vertex being dragged on top of another was with triggers
    private void OnTriggerStay(Collider takeover)
    {
        if (!enabled)
            return;

        // If we collide with something that isn't a vertex, we don't want to continue
        if (takeover.gameObject.tag != "Vertex")
        {
            Debug.Log("Tag != Vertex");
            return;
        }

        materialSwap.material = merge;

        if (pulleyLocomotion.isMovingVertex == true)
        {
            Debug.Log("You need to let go of the vertex first.");
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

        Step step = new Step();
        MeshChange op = new MeshChange(timelineVertices, timelineTriangles);
        step.AddOp(op);
        StepExecutor.instance.AddStep(step);
    }
}

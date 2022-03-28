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

    // Primary and secondary buttons on right hand controller (A and B on Oculus)
    [SerializeField] InputActionReference primaryButtonRef = null;
    [SerializeField] InputActionReference secondaryButtonRef = null;
    bool primaryButtonPressed = false;
    bool secondaryButtonPressed = false;

    // For tool manager, tools should not be active by default
    // Disabling the script only disables start() and update() or something like that
    public bool isEnabled;

    [SerializeField] Material merge;        // yellow
    [SerializeField] Material unselected;   // gray

    // Mesh updating
    public GameObject model;
    Mesh mesh;
    static Vector3[] vertices;
    static int[] triangles;
    static List<int> triangleReferences = new List<int>();
    static List<int> lastVertTriReferences = new List<int>();

    // Vertex lookup
    Vertex mergeVertex;
    static Vertex deleterVertex;
    static Vertex takeoverVertex;
    static int vertex1;
    static int vertex2;
    static int lastIndex;
    static bool connection;

    MeshRenderer materialSwap;
    bool hover;
    public static int chosenMerge = 1;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get MeshFilter to steal triangles
        model = MeshRebuilder.instance.model;
        mesh = model.GetComponent<MeshFilter>().mesh;

        // Stealing triangles (and vertices if we need them)
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        /*
        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        */

        // To reference vertex ids
        mergeVertex = GetComponent<Vertex>();

        // You know what this is
        materialSwap = GetComponent<MeshRenderer>();

        // Hover listeners to change vertex color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);
    }

    // Set material to Merge if we're choosing the second vertex
    void HoverOver(HoverEnterEventArgs arg0)
    {
        if (isEnabled) // Swap isEnabled with 'chosenMerge == 2' if this doesn't work out
            materialSwap.material = merge;

        MeshRebuilder.instance.vertices = mesh.vertices;
        MeshRebuilder.instance.triangles = mesh.triangles;

        hover = true;
    }

    // Not much really to say here tbh
    void HoverExit(HoverExitEventArgs arg0)
    {
        hover = false;
    }

    // InputActions say hello !!
    private void Awake()
    {
        primaryButtonRef.action.started += primaryButtonStart;
        primaryButtonRef.action.canceled += primaryButtonEnd;

        secondaryButtonRef.action.started += secondaryButtonStart;
        secondaryButtonRef.action.canceled += secondaryButtonEnd;
    }

    // InputActions say goodbye :(
    private void OnDestroy()
    {
        primaryButtonRef.action.started -= primaryButtonStart;
        primaryButtonRef.action.canceled -= primaryButtonEnd;

        secondaryButtonRef.action.started -= secondaryButtonStart;
        secondaryButtonRef.action.canceled -= secondaryButtonEnd;
    }

    // Click 1: Select vertex to be merged
    // Click 2: Merge vertex 1 into vertex 2
    // chosenMerge differentiates between these two clicks
    private void primaryButtonStart(InputAction.CallbackContext context)
    {
        primaryButtonPressed = true;

        if (isEnabled)
        {
            // Vertex to be merged
            if (chosenMerge == 1 && hover)
            {
                // Start blinking yellow to indicate merge
                StartCoroutine(ShowMerging());

                // Save id of vertex1
                deleterVertex = mergeVertex;
                vertex1 = deleterVertex.id;

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

                // Update to second click
                chosenMerge = 2;
            }

            // Vertex that vertex 1 will be merged into
            else if (chosenMerge == 2 && hover)
            {
                // Save id of vertex2
                takeoverVertex = mergeVertex;
                vertex2 = takeoverVertex.id;

                // Check to see if vertex2 shares an edge with vertex1
                foreach (Edge connectCheck in takeoverVertex.connectedEdges)
                    if (connectCheck.vert1 == vertex1 || connectCheck.vert2 == vertex1)
                        connection = true;

                // If they don't, kick us out of here
                if (!connection)
                {
                    Debug.Log("You can't merge vertices that don't share an edge!");
                    StopAllCoroutines();
                    deleterVertex.thisVertex.GetComponent<MeshRenderer>().material = unselected;
                    chosenMerge = 1;
                    return;
                }
                connection = false;

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
                        takeoverVertex.connectedEdges.Remove(reconnect);
                        Destroy(reconnect.thisEdge);
                        MeshRebuilder.instance.edgeObjects.Remove(reconnect);
                    }
                    else
                    {
                        // Update vert1 or vert2 ids in the Edge.cs script
                        if (reconnect.vert1 == vertex1)
                            reconnect.vert1 = vertex2;
                        else
                            reconnect.vert2 = vertex2;

                        // Update vert1 or vert2 ids in the Vertex.cs script
                        takeoverVertex.connectedEdges.Add(reconnect);
                    }
                }

                // Delete edges in the same position (overlapping / duplicate) as one of them is no longer needed
                // > If this is not possible, we can do some kind of dynamic deletion by comparing triangle triplets with edge ids
                // > Or brute force it and delete all edges and remake them based on updated triangles array
                // > NEW IDEA: If data from vertex 1 goes to vertex 2, there should be two copies of an edge with the same vertex ids
                //             Look through all edges connected to vertex 2, if two share the same vertex ids, delete one of them

                // Remove vertex1 from the vertices array
                List<Vector3> verticesList = new List<Vector3>();
                verticesList = vertices.ToList();
                verticesList.RemoveAt(vertex1);

                // Update the last vertex to have the same IDs as the one we just deleted (to avoid out of bounds errors later)
                // If the new last vertex is our takeover vertex, all this is unncessary
                if (lastIndex != -1)
                {
                    // Move vertex in last position to the position of the one we just deleted
                    Vector3 lastVertex = verticesList[verticesList.Count - 1];
                    verticesList.RemoveAt(verticesList.Count - 1);
                    verticesList.Insert(vertex1 - 1, lastVertex);

                    // Update the triangles for the newly moved last vertex
                    trianglesList = UpdateTriangles(trianglesList, lastVertTriReferences, vertices.Length - 1, vertex1);

                    // Update IDs in Vertex.cs (Vertex.id) and Edge.cs (Vertex.connectedEdges -> Edge.vert1 or Edge.vert2)
                    Vertex lastVertexGO = GameObject.Find("Vertex" + (vertices.Length - 1)).GetComponent<Vertex>();
                    lastVertexGO.name = "Vertex" + vertex1.ToString();
                    lastVertexGO.id = vertex1;
                    foreach (Edge reconnect in lastVertexGO.connectedEdges)
                    {
                        // Update vert1 or vert2 ids in the Edge.cs script
                        if (reconnect.vert1 == vertices.Length - 1)
                            reconnect.vert1 = vertex1;
                        else
                            reconnect.vert2 = vertex1;
                    }
                }

                // Delete the merged vertex
                Destroy(deleterVertex.thisVertex);
                MeshRebuilder.instance.vertexObjects.Remove(deleterVertex);

                vertices = verticesList.ToArray();
                triangles = trianglesList.ToArray();

                // Update the mesh data and the the edges' direction (repeated code from MoveVertices.UpdateMesh();)
                UpdateMesh(vertex2);

                // Update back to first click
                chosenMerge = 1;
            }
        }
    }

    // Button let go, update chosenMerge to either reset or continue
    private void primaryButtonEnd(InputAction.CallbackContext context)
    {
        primaryButtonPressed = false;
    }

    // Cancels Merge action (only works if first vertex was selected, not the second)
    private void secondaryButtonStart(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = true;

        // Change material back to unselected for all vertices and default back to first click
        StopAllCoroutines();
        materialSwap.material = unselected;
        chosenMerge = 1;
    }

    private void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
    }

    // Vertex selected to be merged will start blinking yellow
    IEnumerator ShowMerging()
    {
        while (true)
        {
            materialSwap.material = merge;
            yield return new WaitForSeconds(0.3f);
            materialSwap.material = unselected;
            yield return new WaitForSeconds(0.3f);
        }
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
        // Vector3[] vertices = MeshRebuilder.instance.vertices;
        // int[] triangles = MeshRebuilder.instance.triangles;

        // Update actual mesh data
        mesh.triangles = triangles;
        mesh.vertices = vertices;       // Error: Mesh.vertices is too small. The supplied vertex array has less vertices than are referenced by the triangles array
        mesh.RecalculateNormals();

        /*
        for (int i = 0; i < vertices.Length; i++)
            Debug.Log("vertices = " + vertices[i]);

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        */

        // Look through visuals Dictionary to update mesh visuals (reconnect edges to vertices)
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
    }
}

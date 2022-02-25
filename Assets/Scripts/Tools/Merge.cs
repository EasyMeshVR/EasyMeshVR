using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Linq;

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
    Vector3[] vertices;
    int[] triangles;
    static List<int> triangleReferences;

    // Vertex lookup
    Vertex mergeVertex;
    static Vertex deleterVertex;
    static Vertex takeoverVertex;
    static int vertex1;
    static int vertex2;
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
        triangleReferences = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("triangles = " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);

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

                // Save id and of vertex 1
                deleterVertex = mergeVertex;
                vertex1 = deleterVertex.id;

                // Get references to all edges the vertex is connected to (easiest to do this through edge ids from Vertex/Edge.cs)
                // I don't think we need to do this since we have access to deleterVertex.connectedEdges

                // Get references to all triangles the vertex is a part of (all adjacent vertices)
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
                // Save id of vertex 2
                takeoverVertex = mergeVertex;
                vertex2 = takeoverVertex.id;

                // Check to see if vertex 2 shares an edge with vertex 1
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

                // Turn the triangles array into a list to remove elements easier
                List<int> trianglesList = triangles.ToList();

                // Determine which triangles will be staying and which will be removed
                // Go backwards through the references so we don't accidentally update while we remove
                for (int i = triangleReferences.Count - 1; i >= 0; i--)
                {
                    // Triangles with only vertex 1 referenced will have those values updated to vertex 2's ids
                    if (trianglesList[triangleReferences[i]] == vertex1)
                    {
                        if (trianglesList[triangleReferences[i] + 1] == vertex2 || trianglesList[triangleReferences[i] + 2] == vertex2)
                            trianglesList.RemoveRange(triangleReferences[i], 3);
                        else
                            trianglesList[triangleReferences[i]] = vertex2;
                    }
                    else if (trianglesList[triangleReferences[i] + 1] == vertex1)
                    {
                        if (trianglesList[triangleReferences[i]] == vertex2 || trianglesList[triangleReferences[i] + 2] == vertex2)
                            trianglesList.RemoveRange(triangleReferences[i], 3);
                        else
                            trianglesList[triangleReferences[i] + 1] = vertex2;
                    }
                    else if (trianglesList[triangleReferences[i] + 2] == vertex1)
                    {
                        if (trianglesList[triangleReferences[i]] == vertex2 || trianglesList[triangleReferences[i] + 1] == vertex2)
                            trianglesList.RemoveRange(triangleReferences[i], 3);
                        else
                            trianglesList[triangleReferences[i] + 2] = vertex2;
                    }
                }

                // Update the triangles array with the new values
                triangles = trianglesList.ToArray();

                // All edges that were connected to vertex 1, connect to vertex 2
                foreach (Edge reconnect in deleterVertex.connectedEdges)
                {
                    // Delete the edge that connects the two vertices
                    if (reconnect.vert1 == vertex2 || reconnect.vert2 == vertex2)
                    {
                        takeoverVertex.connectedEdges.Remove(reconnect);
                        Destroy(reconnect.thisEdge);
                    }
                    else
                    {
                        // Update vert1 or vert2 ids in the Edge.cs and Vertex.cs scripts
                        if (reconnect.vert1 == vertex1)
                            reconnect.vert1 = vertex2;
                        else
                            reconnect.vert2 = vertex2;

                        takeoverVertex.connectedEdges.Add(reconnect);
                    }
                }

                // Delete edges in the same position (overlapping / duplicate) as one of them is no longer needed
                // > If this is not possible, we can do some kind of dynamic deletion by comparing triangle triplets with edge ids
                // > Or brute force it and delete all edges and remake them based on updated triangles array
                // > NEW IDEA: If data from vertex 1 goes to vertex 2, there should be two copies of an edge with the same vertex ids
                //             Look through all edges connected to vertex 2, if two share the same vertex ids, delete one of them

                // Remove vertex1 from the vertices array and delete that vertex's GameObject(I bet there's a faster way to do this)
                List<Vector3> verticesList = new List<Vector3>();
                verticesList = vertices.ToList();
                verticesList.RemoveAt(vertex1);
                vertices = verticesList.ToArray();
                Destroy(deleterVertex.thisVertex);

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
        // Change material back to unselected for all vertices and default back to first click
        StopAllCoroutines();
        materialSwap.material = unselected;
        chosenMerge = 1;
        secondaryButtonPressed = true;
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

    // Update MeshFilter and re-draw in-game visuals
    public void UpdateMesh(int index)
    {
        // Vector3[] vertices = MeshRebuilder.instance.vertices;
        // int[] triangles = MeshRebuilder.instance.triangles;

        // Update actual mesh data
        mesh.triangles = triangles;
        mesh.vertices = vertices;       // Error: Mesh.vertices is too small. The supplied vertex array has less vertices than are referenced by the triangles array
        mesh.RecalculateNormals();

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

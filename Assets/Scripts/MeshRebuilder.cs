using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // this is for line 79

public class MeshRebuilder : MonoBehaviour
{
    GameObject model;

    // Holds the vertex and edge prefabs
    public GameObject vertex;
    public GameObject edge;

    // Mesh data
    Mesh mesh;
    Vector3[] vertices;
    Vector3 vertexPosition;
    int[] triangles;

    // Stores the vertex/edge visual data, i.e. which edges are connected to which vertices
    // Mostly accessed in MoveVertices.cs (and eventually MoveEdges.cs)
    public static Dictionary<GameObject, List<int>> visuals = new Dictionary<GameObject, List<int>>();

    // Setup
    void Awake()
    {
        // For importing in real time we would need the script to get the model automatically
        model = gameObject;
        model.tag = ("Model");

        // Copy vertices and triangles
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Start visualizing the mesh
        RemoveDuplicates();
        CreateVisuals();
    }

    // Deletes the duplicate vertices Unity and STL files create
    // Re-references those duplicate vertices in the triangles array with unique ones only
    void RemoveDuplicates()
    {
        // Filter out unique vertices and triangles, and store indices of every duplicate of a vertex (2 or more dupes)
        HashSet<Vector3> vertexUnique = new HashSet<Vector3>();
        Dictionary<List<int>, Vector3> vertexDuplicate = new Dictionary<List<int>, Vector3>();
        List<int> triangleUnique = new List<int>();

        // Loop over the vertices array, separating duplicates and uniques
        for (int i = 0; i < vertices.Length; i++)
        {
            // List for each index to add 
            List<int> dupeVert = new List<int>();

            // If the hashset already has the vertex, it's a duplicate
            if (vertexUnique.Contains(vertices[i]))
            {
                // If this is not the first duplicate of the vertex, get the previous list, remove the entry, add new index, readd entry
                if (vertexDuplicate.ContainsValue(vertices[i]))
                {
                    List<int> indicies = vertexDuplicate.FirstOrDefault(x => x.Value == vertices[i]).Key;
                    vertexDuplicate.Remove(indicies);
                    indicies.Add(i);
                    vertexDuplicate.Add(indicies, vertices[i]);
                }
                dupeVert.Add(i);
                vertexDuplicate.Add(dupeVert, vertices[i]);
            }
            else
            {
                vertexUnique.Add(vertices[i]);
            }
        }

        // Loop over the triangles array
        for (int i = 0; i < triangles.Length; i++)
        {
            // Check if vertex in triangles array is a duplicate, replace with original if it is
            if (vertexDuplicate.ContainsValue(vertices[triangles[i]]))
            {
                // Need to loop through vertexUnique for imported meshes, for meshes in Unity the first set of vertices are unique
                int j = 0;
                foreach (Vector3 vertex in vertexUnique)
                {
                    if (vertex == vertices[triangles[i]])
                    {
                        triangleUnique.Add(j);
                        break;
                    }
                    j++;
                }
            }
            else
            {
                triangleUnique.Add(triangles[i]);
            }
        }

        // Copy unique vertices to array
        Vector3[] newVertices = new Vector3[vertexUnique.Count];
        int v = 0;
        foreach (Vector3 vertex in vertexUnique)
            newVertices[v++] = vertex;
        vertices = newVertices;

        // Copy unique triangles to array
        int[] newTriangles = new int[triangleUnique.Count];
        int t = 0;
        foreach (int triangle in triangleUnique)
            newTriangles[t++] = triangle;
        triangles = newTriangles;

        // Update the mesh filter with new unique vertex and triangle data
        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.RecalculateNormals();
    }

    // Actually create the vertex and edge GameObject interactables
    void CreateVisuals()
    {
        // Repeats for every vertex stored in the mesh filter
        for (int i = 0; i < vertices.Length; i++)
        {
            // Create a new vertex from a prefab, make it a child of the mesh and set it's position
            GameObject newVertex = Instantiate(vertex, model.transform);
            newVertex.transform.localPosition = vertices[i];

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
                GameObject newEdge = Instantiate(edge, model.transform);

                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[i], vertices[k]);
                newEdge.transform.localPosition = (vertices[i] + vertices[k]) / 2;
                newEdge.transform.localScale = new Vector3(newEdge.transform.localScale.x, edgeDistance, newEdge.transform.localScale.z);

                // Orient the edge to look at the vertices
                newEdge.transform.LookAt(newVertex.transform, Vector3.up);
                newEdge.transform.rotation *= Quaternion.Euler(90, 0, 0);

                // Add edge and it's connecting vertices to a dictionary reference for use in other scripts
                List<int> conVerts = new List<int>();
                conVerts.Add(i);
                conVerts.Add(k);
                visuals.Add(newEdge, conVerts);
            }
        }
    }
}

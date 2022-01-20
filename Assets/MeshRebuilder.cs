using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // this is for line 79

public class MeshRebuilder : MonoBehaviour
{
    // This was an attempt at detecting duplicate vertices and triangles early on and completely rebuilding the mesh data
    // It didn't end up working, sadge
    // Apparently, Unity (and stl) meshes reference EVERY vertex (duplicate or not) while building the triangles
    // If we want to rebuild the mesh from scratch using only unique vertices, we have to build the triangles on our own
    // I'd like to do this as it might make implementing our brushes and tools easier (especially something like knife or extrusion)
    // But we'd either have to find some kind of algorithm or make one ourselves for doing that

    // What I attempted here is:
    //  - line 50: Declare a HashSet to store unique vertices, a Dictionary to reference duplicate vertices, and a List for triangles
    //  - line 55: Detect unique and duplicate vertices and separate them completely
    //  - line 65: Loop over the triangles and keep all triangles triplets that DO NOT reference duplicate vertices
    //  - line 93: Copy the unique vertices and triangles into an array and update the mesh filter

    // Now, *technically*, it all worked, however, here's the kicker
    //  - A normal cube (like the one made in MeshGenerator.cs) has 8 vertices and 12 triangles
    //  - A cube made by Unity has 24 vertices and 12 triangles
    //  - The issue with the Unity cube is that it doesn't duplicate triangles like it does vertices
    //  - It's like it ends up evenly using the vertices to make the triangles, which is weird
    // There's some Debug statements you can uncomment to see exactly what I mean if this doesn't make sense (line 47-48, 89-90)

    // The mesh or model the vertices/edges are spawning on
    //public GameObject model;
    // Holds the vertex and edge prefabs
    public GameObject vertex;
    public GameObject edge;

    // Mesh data
    Mesh mesh;
    Vector3[] vertices;
    Vector3 vertexPosition;
    int[] triangles;

    public void Awake()
    {
        // For importing in real time we would need the script to get the model automatically
        GameObject model = gameObject;
        model.tag = ("Model");

        // Copy vertices and triangles
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
            Debug.Log("original triangle " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);

        HashSet<Vector3> vertexUnique = new HashSet<Vector3>();
        //Dictionary<int, Vector3> vertexDuplicate = new Dictionary<int, Vector3>();
        
        // Stores indicies of every duplicate of a vertex (2 or more dupes)
        Dictionary<List<int>, Vector3> vertexDuplicate = new Dictionary<List<int>, Vector3>();

        List<int> triangleUnique = new List<int>();

        // Loop over the vertices array, separating duplicates and uniques
        for (int i = 0; i < vertices.Length; i++)
        {
            // List for each index to add 
            List<int> dupeVert = new List<int>();

            /*
            // If the hashset already has the vertex, it's a duplicate
            if (vertexUnique.Contains(vertices[i]))
                vertexDuplicate.Add(i, vertices[i]);
            else
                vertexUnique.Add(vertices[i]);
            */

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

        /*
        print("old verts " + vertices.Length);
        print("unique " + vertexUnique.Count);
        foreach(Vector3 vert in vertexUnique)
            print("vertex at " + vert);

        foreach (KeyValuePair<List<int>, Vector3> kvp in vertexDuplicate)
        {
            foreach(int key in kvp.Key)
                print("dupe index " + key);
            print(" Vertex " +  kvp.Value  );
        }
        */

        // Loop over the triangles array
        for (int i = 0; i < triangles.Length; i++)
        {
            // print(i);
            // print("vertex " + vertices[triangles[i]]);

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

        // print("old trs " + triangles.Length);
        // print("unique " + triangleUnique.Count);

        for (int i = 0; i < triangleUnique.Count; i += 3)
             Debug.Log("unique triangle " + triangleUnique[i] + ", " + triangleUnique[i + 1] + ", " + triangleUnique[i + 2]);

        // Copy unique vertices to array
        Vector3[] newVertices = new Vector3[vertexUnique.Count];
        int v = 0;
        foreach (Vector3 vertex in vertexUnique)
            newVertices[v++] = vertex;
        vertices = newVertices;

        foreach(Vector3 vert in newVertices)
            print("vertex at " + vert);

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

        // Repeats for every vertex stored in the mesh filter
        for (int i = 0; i < vertices.Length; i++)
        {
            // Saves the position of the current vertex
            vertexPosition = vertices[i];

            // Create a new vertex from a prefab, make it a child of the mesh and set it's position
            GameObject newVertex = Instantiate(vertex);
            newVertex.transform.SetParent(model.transform);
            newVertex.transform.localPosition = vertexPosition;

            // --------------------------------------------------------------------------------------

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
                GameObject newEdge = Instantiate(edge);
                newEdge.transform.SetParent(model.transform);

                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[i], vertices[k]);
                newEdge.transform.localPosition = (vertices[i] + vertices[k]) / 2;
                newEdge.transform.localScale = new Vector3(newEdge.transform.localScale.x, edgeDistance, newEdge.transform.localScale.z);

                // Orient the edge to look at the vertices
                newEdge.transform.LookAt(newVertex.transform, Vector3.up);
                newEdge.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }
    }
}

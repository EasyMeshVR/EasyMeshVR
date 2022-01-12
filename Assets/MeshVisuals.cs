using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVisuals : MonoBehaviour
{
    // Everything should be commented to the point someone can understand it without me
    // If not I'm sorry

    // Lots of inefficiency in here, gotta clean that up
    // The book I wrote in the discord is also helpful for cleaning up all the duplicate stuff we're spawning in
    // Ethan says to try option #2 first;
    // - instantiate the same way as below but ignore vertices with duplicate positions when spawning the visuals
    // - but still update each and every vertex in the mesh filter vertices array

    // You most likely won't need to, but if you want to use this script on the MeshGenerator;
    // - just drag the MeshGenerator prefab into the game scene
    // - change public void Awake() to public void SpawnVertices()
    // - and uncomment line 25 in the MeshGenerator script
    // - (you have to do that or else nothing will spawn since they both execute at the same time instead of one after the other)
    // Otherwise, just attach this to whatever model you want to spawn stuff on

    // The mesh or model the vertices/edges are spawning on
    public GameObject model;

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
        // Copy vertices and triangles
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

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
            for (int j = 0; j < triangles.Length; j+=3)
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

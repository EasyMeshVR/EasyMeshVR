using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class MeshGenerator : MonoBehaviour
{
    public MeshVisuals meshVisuals;

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        UpdateMesh();

        // meshVisuals.SpawnVertices();
    }

    void CreateMesh()
    {
        vertices = new Vector3[]
        {
            new Vector3 (0,0,0), // 0
            new Vector3 (0,0,1), // 1
            new Vector3 (1,0,0), // 2
            new Vector3 (1,0,1), // 3
            new Vector3 (0,1,0), // 4
            new Vector3 (0,1,1), // 5
            new Vector3 (1,1,0), // 6
            new Vector3 (1,1,1)  // 7
        };

        triangles = new int[]
        {
            0,2,3,
            3,1,0,
            0,1,5,
            5,4,0,
            1,3,7,
            7,5,1,
            3,2,6,
            6,7,3,
            2,0,4,
            4,6,2,
            6,4,5,
            5,7,6
        };
    }

    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}

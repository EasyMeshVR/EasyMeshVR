using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshChange : IOperation
{
    MeshRebuilder meshRebuilder = GameObject.FindObjectOfType<MeshRebuilder>();
    Vector3[] oldVertices, newVertices;
    int[] oldTriangles, newTriangles;

    public MeshChange(Vector3[] inputVertices, int[] inputTriangles)
    {
        oldVertices = inputVertices;
        newVertices = meshRebuilder.model.GetComponent<MeshFilter>().mesh.vertices;

        oldTriangles = inputTriangles;
        newTriangles = meshRebuilder.model.GetComponent<MeshFilter>().mesh.triangles;
    }

    public void Execute()
    {
        meshRebuilder.model.GetComponent<MeshFilter>().mesh.vertices = newVertices;
        meshRebuilder.model.GetComponent<MeshFilter>().mesh.triangles = newTriangles;
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        meshRebuilder.model.GetComponent<MeshFilter>().mesh.vertices = oldVertices;
        meshRebuilder.model.GetComponent<MeshFilter>().mesh.triangles = oldTriangles;
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

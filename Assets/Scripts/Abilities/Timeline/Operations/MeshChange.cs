using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class MeshChange : IOperation
{
    MeshRebuilder meshRebuilder;
    Mesh mesh;
    int meshId, deleterVertexId, takeoverVertexId;
    Vector3[] oldVertices, newVertices;
    int[] oldTriangles, newTriangles;

    public MeshChange(Vector3[] inputVertices, int[] inputTriangles, int meshId, int deleterVertexId, int takeoverVertexId)
    {
        this.meshId = meshId;
        this.deleterVertexId = deleterVertexId;
        this.takeoverVertexId = takeoverVertexId;
        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
        mesh = meshRebuilder.model.GetComponent<MeshFilter>().mesh;

        oldVertices = inputVertices;
        newVertices = mesh.vertices;

        oldTriangles = inputTriangles;
        newTriangles = mesh.triangles;
    }

    public void Execute()
    {
        Vertex deleterVertex = meshRebuilder.vertexObjects[deleterVertexId];

        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;

        meshRebuilder.vertices = newVertices;
        meshRebuilder.triangles = newTriangles;

        deleterVertex.GetComponent<Merge>().MergeVertex(new MergeVertexEvent
        {
            deleterVertexId = deleterVertexId,
            takeOverVertexId = takeoverVertexId,
            meshId = meshId
        });
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        mesh.vertices = oldVertices;
        mesh.triangles = oldTriangles;

        meshRebuilder.vertices = oldVertices;
        meshRebuilder.triangles = oldTriangles;

        meshRebuilder.removeVisuals();
        meshRebuilder.CreateVisuals();
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

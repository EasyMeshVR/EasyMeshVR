using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class MergeVertexOp : IOperation
{
    MeshRebuilder meshRebuilder;
    Mesh mesh;
    int meshId, deleterVertexId, takeoverVertexId;

    public MergeVertexOp(int meshId, int deleterVertexId, int takeoverVertexId)
    {
        this.meshId = meshId;
        this.deleterVertexId = deleterVertexId;
        this.takeoverVertexId = takeoverVertexId;

        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
        mesh = meshRebuilder.model.GetComponent<MeshFilter>().mesh;
    }

    public void Execute()
    {
        Vertex deleterVertex = meshRebuilder.vertexObjects[deleterVertexId];
        Vertex takeoverVertex = meshRebuilder.vertexObjects[takeoverVertexId];

        MergeVertexEvent mergeVertexEvent = new MergeVertexEvent
        {
            deleterVertexId = deleterVertexId,
            takeOverVertexId = takeoverVertexId,
            meshId = meshId
        };

        Merge deleterMerge = deleterVertex.GetComponent<Merge>();
        deleterMerge.MergeVertex(mergeVertexEvent);
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        Vertex deleterVertex = meshRebuilder.vertexObjects[deleterVertexId];
        Vertex takeoverVertex = meshRebuilder.vertexObjects[takeoverVertexId];


    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

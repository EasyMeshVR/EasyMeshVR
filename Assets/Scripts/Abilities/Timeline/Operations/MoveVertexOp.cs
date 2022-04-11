using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class MoveVertexOp : IOperation
{
    MeshRebuilder meshRebuilder;
    int meshId, vertexId;
    Vector3 oldPosition, newPosition;

    public MoveVertexOp(int meshId, int vertexId, Vector3 oldPosition, Vector3 newPosition)
    {
        this.meshId = meshId;
        this.vertexId = vertexId;
        this.oldPosition = oldPosition;
        this.newPosition = newPosition;

        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
    }

    public void Execute()
    {
        Vertex vertexObj = meshRebuilder.vertexObjects[vertexId];
        MoveVertices moveVertices = vertexObj.GetComponent<MoveVertices>();

        vertexObj.transform.localPosition = newPosition;
        meshRebuilder.vertices[vertexId] = newPosition;
        moveVertices.UpdateMesh(vertexId);

        // Debug.LogFormat("MoveVertexOp(): Execute on meshId {0} vertexId {1}", meshId, vertexId);
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        Vertex vertexObj = meshRebuilder.vertexObjects[vertexId];
        MoveVertices moveVertices = vertexObj.GetComponent<MoveVertices>();

        vertexObj.transform.localPosition = oldPosition;
        meshRebuilder.vertices[vertexId] = oldPosition;
        moveVertices.UpdateMesh(vertexId);

        // Debug.LogFormat("MoveVertexOp(): Deexecute on meshId {0} vertexId {1}", meshId, vertexId);
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

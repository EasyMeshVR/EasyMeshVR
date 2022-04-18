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
        if (!MoveVertexIdsInBounds(vertexId))
        {
            Debug.LogWarning("Warning: MoveVertexOp Deexecute(): vertexId is not in bounds!");
            return;
        }

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

    bool VertexIdInBounds(int id)
    {
        return id >= 0 && id < meshRebuilder.vertexObjects.Count;
    }

    public bool MoveVertexIdsInBounds(int vertexId)
    {
        if (!VertexIdInBounds(vertexId))
        {
            Debug.LogWarningFormat("Warning: MoveVertexOp: vertexId {0} was out of bounds of vertexObjects of length {1}", vertexId, meshRebuilder.vertexObjects.Count);
            return false;
        }

        return true;
    }

    public void Deexecute()
    {
        if (!MoveVertexIdsInBounds(vertexId))
        {
            Debug.LogWarning("Warning: MoveVertexOp Deexecute(): vertexId is not in bounds!");
            return;
        }

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

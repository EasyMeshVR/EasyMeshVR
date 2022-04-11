using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class MoveEdgeOp : IOperation
{
    Edge edgeObj;
    Vertex vert1Obj, vert2Obj;
    MoveEdge moveEdge;
    MeshRebuilder meshRebuilder;
    int meshId, edgeId;
    Vector3 oldEdgePosition, newEdgePosition;
    Vector3 oldVert1Position, newVert1Position;
    Vector3 oldVert2Position, newVert2Position;

    public MoveEdgeOp(int meshId, int edgeId, Vector3 oldEdgePosition, Vector3 newEdgePosition, Vector3 oldVert1Position,
                      Vector3 newVert1Position, Vector3 oldVert2Position, Vector3 newVert2Position)
    {
        this.meshId = meshId;
        this.edgeId = edgeId;
        this.oldEdgePosition = oldEdgePosition;
        this.newEdgePosition = newEdgePosition;
        this.oldVert1Position = oldVert1Position;
        this.newVert1Position = newVert1Position;
        this.oldVert2Position = oldVert2Position;
        this.newVert2Position = newVert2Position;

        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
        edgeObj = meshRebuilder.edgeObjects[edgeId];
        vert1Obj = meshRebuilder.vertexObjects[edgeObj.vert1];
        vert2Obj = meshRebuilder.vertexObjects[edgeObj.vert2];
        moveEdge = edgeObj.GetComponent<MoveEdge>();
    }

    public void Execute()
    {
        edgeObj.transform.localPosition = newEdgePosition;
        vert1Obj.transform.localPosition = newVert1Position;
        vert2Obj.transform.localPosition = newVert2Position;
        meshRebuilder.vertices[edgeObj.vert1] = newVert1Position;
        meshRebuilder.vertices[edgeObj.vert2] = newVert2Position;
        moveEdge.UpdateMesh(edgeId, edgeObj.vert1, edgeObj.vert2, false);

        // Debug.LogFormat("MoveEdgeOp(): Execute on meshId {0} edgeId {1}", meshId, edgeId);
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        edgeObj.transform.localPosition = oldEdgePosition;
        vert1Obj.transform.localPosition = oldVert1Position;
        vert2Obj.transform.localPosition = oldVert2Position;
        meshRebuilder.vertices[edgeObj.vert1] = oldVert1Position;
        meshRebuilder.vertices[edgeObj.vert2] = oldVert2Position;
        moveEdge.UpdateMesh(edgeId, edgeObj.vert1, edgeObj.vert2, false);

        // Debug.LogFormat("MoveEdgeOp(): Deexecute on meshId {0} edgeId {1}", meshId, edgeId);
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

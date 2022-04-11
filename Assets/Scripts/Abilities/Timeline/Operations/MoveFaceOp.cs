using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class MoveFaceOp : IOperation
{
    MeshRebuilder meshRebuilder;
    int meshId, faceId;
    Vector3 oldFacePosition, newFacePosition;
    Vector3 oldVert1Position, newVert1Position;
    Vector3 oldVert2Position, newVert2Position;
    Vector3 oldVert3Position, newVert3Position;
    Vector3 oldEdge1Position, newEdge1Position;
    Vector3 oldEdge2Position, newEdge2Position;
    Vector3 oldEdge3Position, newEdge3Position;

    public MoveFaceOp(int meshId, int faceId, Vector3 oldFacePosition, Vector3 newFacePosition, Vector3 oldVert1Position,
                      Vector3 newVert1Position, Vector3 oldVert2Position, Vector3 newVert2Position, Vector3 oldVert3Position,
                      Vector3 newVert3Position, Vector3 oldEdge1Position, Vector3 newEdge1Position, Vector3 oldEdge2Position,
                      Vector3 newEdge2Position, Vector3 oldEdge3Position, Vector3 newEdge3Position)
    {
        this.meshId = meshId;
        this.faceId = faceId;
        this.oldFacePosition = oldFacePosition;
        this.newFacePosition = newFacePosition;

        this.oldVert1Position = oldVert1Position;
        this.newVert1Position = newVert1Position;

        this.oldVert2Position = oldVert2Position;
        this.newVert2Position = newVert2Position;

        this.oldVert3Position = oldVert3Position;
        this.newVert3Position = newVert3Position;

        this.oldEdge1Position = oldEdge1Position;
        this.newEdge1Position = newEdge1Position;

        this.oldEdge2Position = oldEdge2Position;
        this.newEdge2Position = newEdge2Position;

        this.oldEdge3Position = oldEdge3Position;
        this.newEdge3Position = newEdge3Position;

        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
    }

    public void Execute()
    {
        Face faceObj = meshRebuilder.faceObjects[faceId];
        Vertex vert1Obj = meshRebuilder.vertexObjects[faceObj.vert1];
        Vertex vert2Obj = meshRebuilder.vertexObjects[faceObj.vert2];
        Vertex vert3Obj = meshRebuilder.vertexObjects[faceObj.vert3];
        Edge edge1Obj = meshRebuilder.edgeObjects[faceObj.edge1];
        Edge edge2Obj = meshRebuilder.edgeObjects[faceObj.edge2];
        Edge edge3Obj = meshRebuilder.edgeObjects[faceObj.edge3];
        MoveFace moveFace = faceObj.GetComponent<MoveFace>();

        faceObj.transform.localPosition = newFacePosition;
        vert1Obj.transform.localPosition = newVert1Position;
        vert2Obj.transform.localPosition = newVert2Position;
        vert3Obj.transform.localPosition = newVert3Position;
        edge1Obj.transform.localPosition = newEdge1Position;
        edge2Obj.transform.localPosition = newEdge2Position;
        edge3Obj.transform.localPosition = newEdge3Position;

        meshRebuilder.vertices[faceObj.vert1] = newVert1Position;
        meshRebuilder.vertices[faceObj.vert2] = newVert2Position;
        meshRebuilder.vertices[faceObj.vert3] = newVert3Position;

        moveFace.UpdateMesh(faceObj.vert1, faceObj.vert2, faceObj.vert3, false);

        // Debug.LogFormat("MoveFaceOp(): Execute on meshId {0} faceId {1}", meshId, faceId);
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        Face faceObj = meshRebuilder.faceObjects[faceId];
        Vertex vert1Obj = meshRebuilder.vertexObjects[faceObj.vert1];
        Vertex vert2Obj = meshRebuilder.vertexObjects[faceObj.vert2];
        Vertex vert3Obj = meshRebuilder.vertexObjects[faceObj.vert3];
        Edge edge1Obj = meshRebuilder.edgeObjects[faceObj.edge1];
        Edge edge2Obj = meshRebuilder.edgeObjects[faceObj.edge2];
        Edge edge3Obj = meshRebuilder.edgeObjects[faceObj.edge3];
        MoveFace moveFace = faceObj.GetComponent<MoveFace>();

        faceObj.transform.localPosition = oldFacePosition;
        vert1Obj.transform.localPosition = oldVert1Position;
        vert2Obj.transform.localPosition = oldVert2Position;
        vert3Obj.transform.localPosition = oldVert3Position;
        edge1Obj.transform.localPosition = oldEdge1Position;
        edge2Obj.transform.localPosition = oldEdge2Position;
        edge3Obj.transform.localPosition = oldEdge3Position;

        meshRebuilder.vertices[faceObj.vert1] = oldVert1Position;
        meshRebuilder.vertices[faceObj.vert2] = oldVert2Position;
        meshRebuilder.vertices[faceObj.vert3] = oldVert3Position;

        moveFace.UpdateMesh(faceObj.vert1, faceObj.vert2, faceObj.vert3, false);

        // Debug.LogFormat("MoveFaceOp(): Deexecute on meshId {0} faceId {1}", meshId, faceId);
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

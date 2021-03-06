using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyMeshVR.Multiplayer;

public class ExtrudeOp : IOperation
{
    Face faceObj;
    MoveFace moveFace;
    MeshRebuilder meshRebuilder;
    Mesh mesh;
    Extrude.ExtrudedObjects extrudedObjects;
    int meshId, faceId;
    float extrudeDistance;
    bool sendFaceExtrudeEvent;

    public ExtrudeOp(int meshId, int faceId, float extrudeDistance, bool sendFaceExtrudeEvent)
    {
        this.meshId = meshId;
        this.faceId = faceId;
        this.extrudeDistance = extrudeDistance;
        this.sendFaceExtrudeEvent = sendFaceExtrudeEvent;

        meshRebuilder = NetworkMeshManager.instance.meshRebuilders[meshId];
        mesh = meshRebuilder.model.GetComponent<MeshFilter>().mesh;
        faceObj = meshRebuilder.faceObjects[faceId];
        moveFace = faceObj.GetComponent<MoveFace>();
    }

    public void Execute()
    {
        Extrude extrudeTool = (SwitchControllers.instance.rayActive) ? ToolManager.instance.extrudeScriptRay : ToolManager.instance.extrudeScriptGrab;
        extrudedObjects = extrudeTool.extrudeFace(faceId, meshRebuilder, mesh, extrudeDistance, sendFaceExtrudeEvent);

        // Set the sendFaceExtrudeEvent to false after sending, since we only want to send it once and
        // let the redo timeline event handle re-creating the mesh
        if (sendFaceExtrudeEvent) sendFaceExtrudeEvent = false;

        // Debug.LogFormat("ExtrudeOp(): Execute on meshId {0} faceId {1}", meshId, faceId);
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        List<Vector3> newVertsList = new List<Vector3>(meshRebuilder.vertices);
        HashSet<int> deletedEdgeIds = new HashSet<int>();
        HashSet<int> deletedFaceIds = new HashSet<int>();
        int removedVertsCount = 0;
        int vertLen = meshRebuilder.vertexObjects.Count;

        foreach (int vertexId in extrudedObjects.newVertexIds)
        {
            Vertex vertexObj = meshRebuilder.vertexObjects[vertexId];

            // Destroy connected edges, and be careful not to call destroy on an already deleted edge
            foreach (Edge edge in vertexObj.connectedEdges)
            {
                if (!deletedEdgeIds.Contains(edge.id))
                {
                    GameObject.Destroy(edge.gameObject);
                    meshRebuilder.edgeObjects.Remove(edge);
                    deletedEdgeIds.Add(edge.id);

                    // Remove connectedEdges from adjacent vertices' array
                    if (!extrudedObjects.newVertexIds.Contains(edge.vert1))
                    {
                        Vertex adjacentVertex = meshRebuilder.vertexObjects[edge.vert1];
                        adjacentVertex.connectedEdges.Remove(edge);
                    }
                    if (!extrudedObjects.newVertexIds.Contains(edge.vert2))
                    {
                        Vertex adjacentVertex = meshRebuilder.vertexObjects[edge.vert2];
                        adjacentVertex.connectedEdges.Remove(edge);
                    }
                }
            }

            // Destroy connected faces, and be careful not to call destroy on already deleted face
            foreach (Face face in vertexObj.connectedFaces)
            {
                if (!deletedFaceIds.Contains(face.id) && face != null)
                {
                    GameObject.Destroy(face.gameObject);
                    meshRebuilder.faceObjects.Remove(face);
                    deletedFaceIds.Add(face.id);

                    // Remove connectedFaces from adjacent vertices' array
                    if (!extrudedObjects.newVertexIds.Contains(face.vert1) && face.vert1 < vertLen && face.vert1 >= 0)
                    {
                        Vertex adjacentVertex = meshRebuilder.vertexObjects[face.vert1];
                        adjacentVertex.connectedFaces.Remove(face);
                    }
                    if (!extrudedObjects.newVertexIds.Contains(face.vert2) && face.vert2 < vertLen && face.vert2 >= 0)
                    {
                        Vertex adjacentVertex = meshRebuilder.vertexObjects[face.vert2];
                        adjacentVertex.connectedFaces.Remove(face);
                    }
                    if (!extrudedObjects.newVertexIds.Contains(face.vert3) && face.vert3 < vertLen && face.vert3 >= 0)
                    {
                        Vertex adjacentVertex = meshRebuilder.vertexObjects[face.vert3];
                        adjacentVertex.connectedFaces.Remove(face);
                    }
                }
            }
        }

        // Delete the vertex references
        foreach (int vertexId in extrudedObjects.newVertexIds)
        {
            Vertex vertexObj = meshRebuilder.vertexObjects[vertexId - removedVertsCount];
            GameObject.Destroy(vertexObj.gameObject);
            meshRebuilder.vertexObjects.RemoveAt(vertexId - removedVertsCount);
            newVertsList.RemoveAt(vertexId - removedVertsCount);
            removedVertsCount++;
        }

        mesh.Clear();

        // Update vertices array
        Vector3[] verts = newVertsList.ToArray();
        mesh.vertices = verts;
        meshRebuilder.vertices = verts;

        // Update triangles array, removing the triangles generated by this Extrusion operation
        List<int> newTrianglesList = new List<int>(meshRebuilder.triangles);
        newTrianglesList.RemoveRange(extrudedObjects.newTriangleIndexStart, extrudedObjects.newTriangleCount);

        int[] tris = newTrianglesList.ToArray();
        mesh.triangles = tris;
        meshRebuilder.triangles = tris;
        mesh.RecalculateNormals();

        // Debug.LogFormat("ExtrudeOp(): Deexecute on meshId {0} faceId {1}", meshId, faceId);
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMeshVR.Multiplayer
{
    public abstract class NetworkEvent
    {
        public int actorNumber { get; set; }
        public bool isCached { get; set; } = false;
        public bool released { get; set; } = false;
    }

    public class VertexPullEvent : NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public Vector3 oldVertexPos { get; set; }
        public Vector3 vertexPos { get; set; }

        public static object[] SerializeEvent(VertexPullEvent vertexEvent)
        {
            return new object[]
            {
               vertexEvent.id,
               vertexEvent.vertexPos,
               vertexEvent.released,
               vertexEvent.actorNumber,
               vertexEvent.meshId,
               vertexEvent.oldVertexPos
            };
        }

        public static VertexPullEvent DeserializeEvent(object[] data)
        {
            return new VertexPullEvent()
            {
                id = (int)data[0],
                vertexPos = (Vector3)data[1],
                released = (bool)data[2],
                actorNumber = (int)data[3],
                meshId = (int)data[4],
                oldVertexPos = (Vector3)data[5]
            };
        }
    }

    public class EdgePullEvent : NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public int vert1 { get; set; }
        public int vert2 { get; set; }
        public Vector3 oldPosition { get; set; }
        public Vector3 position { get; set; }
        public Vector3 oldVertex1Pos { get; set; }
        public Vector3 vertex1Pos { get; set; }
        public Vector3 oldVertex2Pos { get; set; }
        public Vector3 vertex2Pos { get; set; }

        public static object[] SerializeEvent(EdgePullEvent edgeEvent)
        {
            return new object[]
            {
                edgeEvent.id,
                edgeEvent.vert1,
                edgeEvent.vert2,
                edgeEvent.position,
                edgeEvent.vertex1Pos,
                edgeEvent.vertex2Pos,
                edgeEvent.released,
                edgeEvent.actorNumber,
                edgeEvent.meshId,
                edgeEvent.oldPosition,
                edgeEvent.oldVertex1Pos,
                edgeEvent.oldVertex2Pos
            };
        }

        public static EdgePullEvent DeserializeEvent(object[] data)
        {
            EdgePullEvent edgeEvent = new EdgePullEvent()
            {
                id = (int)data[0],
                vert1 = (int)data[1],
                vert2 = (int)data[2],
                position = (Vector3)data[3],
                vertex1Pos = (Vector3)data[4],
                vertex2Pos = (Vector3)data[5],
                released = (bool)data[6],
                actorNumber = (int)data[7],
                meshId = (int)data[8],
                oldPosition = (Vector3)data[9],
                oldVertex1Pos = (Vector3)data[10],
                oldVertex2Pos = (Vector3)data[11]
            };

            return edgeEvent;
        }
    }

    public class FacePullEvent : NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public int vert1 { get; set; }
        public int vert2 { get; set; }
        public int vert3 { get; set; }
        public int edge1 { get; set; }
        public int edge2 { get; set; }
        public int edge3 { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 oldPosition { get; set; }
        public Vector3 position { get; set; }
        public Vector3 oldVertex1Pos { get; set; }
        public Vector3 vertex1Pos { get; set; }
        public Vector3 oldVertex2Pos { get; set; }
        public Vector3 vertex2Pos { get; set; }
        public Vector3 oldVertex3Pos { get; set; }
        public Vector3 vertex3Pos { get; set; }
        public Vector3 oldEdge1Pos { get; set; }
        public Vector3 edge1Pos { get; set; }
        public Vector3 oldEdge2Pos { get; set; }
        public Vector3 edge2Pos { get; set; }
        public Vector3 oldEdge3Pos { get; set; }
        public Vector3 edge3Pos { get; set; }

        public static object[] SerializeEvent(FacePullEvent faceEvent)
        {
            return new object[]
            {
                faceEvent.id,
                faceEvent.vert1,
                faceEvent.vert2,
                faceEvent.vert3,
                faceEvent.edge1,
                faceEvent.edge2,
                faceEvent.edge3,
                faceEvent.normal,
                faceEvent.position,
                faceEvent.vertex1Pos,
                faceEvent.vertex2Pos,
                faceEvent.vertex3Pos,
                faceEvent.released,
                faceEvent.actorNumber,
                faceEvent.meshId,
                faceEvent.oldPosition,
                faceEvent.oldVertex1Pos,
                faceEvent.oldVertex2Pos,
                faceEvent.oldVertex3Pos,
                faceEvent.oldEdge1Pos,
                faceEvent.oldEdge2Pos,
                faceEvent.oldEdge3Pos,
                faceEvent.edge1Pos,
                faceEvent.edge2Pos,
                faceEvent.edge3Pos,
            };
        }

        public static FacePullEvent DeserializeEvent(object[] data)
        {
            FacePullEvent faceEvent = new FacePullEvent()
            {
                id = (int)data[0],
                vert1 = (int)data[1],
                vert2 = (int)data[2],
                vert3 = (int)data[3],
                edge1 = (int)data[4],
                edge2 = (int)data[5],
                edge3 = (int)data[6],
                normal = (Vector3)data[7],
                position = (Vector3)data[8],
                vertex1Pos = (Vector3)data[9],
                vertex2Pos = (Vector3)data[10],
                vertex3Pos = (Vector3)data[11],
                released = (bool)data[12],
                actorNumber = (int)data[13],
                meshId = (int)data[14],
                oldPosition = (Vector3)data[15],
                oldVertex1Pos = (Vector3)data[16],
                oldVertex2Pos = (Vector3)data[17],
                oldVertex3Pos = (Vector3)data[18],
                oldEdge1Pos = (Vector3)data[19],
                oldEdge2Pos = (Vector3)data[20],
                oldEdge3Pos = (Vector3)data[21],
                edge1Pos = (Vector3)data[22],
                edge2Pos = (Vector3)data[23],
                edge3Pos = (Vector3)data[24]
            };

            return faceEvent;
        }
    }

    public class FaceExtrudeEvent : NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public float extrudeDistance { get; set; }

        public static object[] SerializeEvent(FaceExtrudeEvent faceExtrudeEvent)
        {
            return new object[]
            {
                faceExtrudeEvent.id,
                faceExtrudeEvent.released,
                faceExtrudeEvent.actorNumber,
                faceExtrudeEvent.meshId,
                faceExtrudeEvent.extrudeDistance
            };
        }

        public static FaceExtrudeEvent DeserializeEvent(object[] data)
        {
            return new FaceExtrudeEvent
            {
                id = (int)data[0],
                released = (bool)data[1],
                actorNumber = (int)data[2],
                meshId = (int)data[3],
                extrudeDistance = (float)data[4]
            };
        }
    }

    public class VertexLockEvent : NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public bool locked { get; set; }

        public static object[] SerializeEvent(VertexLockEvent vertexLockEvent)
        {
            return new object[]
            {
                vertexLockEvent.id,
                vertexLockEvent.meshId,
                vertexLockEvent.actorNumber,
                vertexLockEvent.locked
            };
        }

        public static VertexLockEvent DeserializeEvent(object[] data)
        {
            return new VertexLockEvent
            {
                id = (int)data[0],
                meshId = (int)data[1],
                actorNumber = (int)data[2],
                locked = (bool)data[3]
            };
        }
    }

    public class MergeVertexEvent: NetworkEvent
    {
        public int deleterVertexId { get; set; }
        public int takeOverVertexId { get; set; }
        public int meshId { get; set; }
        
        public static object[] SerializeEvent(MergeVertexEvent mergeVertexEvent)
        {
            return new object[]
            {
                mergeVertexEvent.deleterVertexId,
                mergeVertexEvent.takeOverVertexId,
                mergeVertexEvent.meshId,
                mergeVertexEvent.actorNumber
            };
        }

        public static MergeVertexEvent DeserializeEvent(object[] data)
        {
            return new MergeVertexEvent
            {
                deleterVertexId = (int)data[0],
                takeOverVertexId = (int)data[1],
                meshId = (int)data[2],
                actorNumber = (int)data[3]
            };
        }
    }

    public class UndoTimelineEvent : NetworkEvent
    {
        public static object[] SerializeEvent(UndoTimelineEvent undoTimelineEvent)
        {
            return new object[]
            {
                undoTimelineEvent.actorNumber
            };
        }

        public static UndoTimelineEvent DeserializeEvent(object[] data)
        {
            return new UndoTimelineEvent
            {
                actorNumber = (int)data[0]
            };
        }
    }

    public class RedoTimelineEvent : NetworkEvent
    {
        public static object[] SerializeEvent(RedoTimelineEvent redoTimelineEvent)
        {
            return new object[]
            {
                redoTimelineEvent.actorNumber
            };
        }

        public static RedoTimelineEvent DeserializeEvent(object[] data)
        {
            return new RedoTimelineEvent
            {
                actorNumber = (int)data[0]
            };
        }
    }

    public class ChaneLightColorEvent : NetworkEvent
    {
        public Vector3 colorVec { get; set; }

        public static object[] SerializeEvent(ChaneLightColorEvent redoTimelineEvent)
        {
            return new object[]
            {
                redoTimelineEvent.actorNumber,
                redoTimelineEvent.colorVec
            };
        }

        public static ChaneLightColorEvent DeserializeEvent(object[] data)
        {
            return new ChaneLightColorEvent
            {
                actorNumber = (int)data[0],
                colorVec = (Vector3)data[1]
            };
        }
    }
}

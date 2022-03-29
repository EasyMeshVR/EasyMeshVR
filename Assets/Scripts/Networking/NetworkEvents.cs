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

    public class VertexPullEvent: NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public Vector3 vertexPos { get; set; }

        public static object[] SerializeEvent(VertexPullEvent vertexEvent)
        {
            return new object[]
            {
               vertexEvent.id,
               vertexEvent.vertexPos,
               vertexEvent.released,
               vertexEvent.actorNumber,
               vertexEvent.meshId
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
                meshId = (int)data[4]
            };
        }
    }

    public class EdgePullEvent: NetworkEvent
    {
        public int id { get; set; }
        public int meshId { get; set; }
        public int vert1 { get; set; }
        public int vert2 { get; set; }
        public Vector3 position { get; set; }
        public Vector3 vertex1Pos { get; set; }
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
                edgeEvent.meshId
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
                meshId = (int)data[8]
            };

            return edgeEvent;
        }
    }
}
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMeshVR.Multiplayer
{
    public class VertexPullEvent
    {
        public int id { get; set; }
        public Vector3 vertexPos { get; set; }
        public bool isCached { get; set; } = false;
        public bool released { get; set; } = false;

        // TODO
        public static object[] SerializeEvent(VertexPullEvent edgeEvent)
        {
            return new object[]
            {
                
            };
        }

        // TODO
        public static VertexPullEvent DeserializeEvent(object[] data)
        {
            return new VertexPullEvent()
            {
                
            };
        }
    }

    public class EdgePullEvent
    {
        public int id { get; set; }
        public int vert1 { get; set; }
        public int vert2 { get; set; }
        public Vector3 position { get; set; }
        public Vector3 vertex1Pos { get; set; }
        public Vector3 vertex2Pos { get; set; }
        public bool isCached { get; set; } = false;
        public bool released { get; set; } = false;

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
                released = (bool)data[6]
            };

            return edgeEvent;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMeshVR.Core
{
    public static class Constants
    {
        public const byte MAX_PLAYERS_PER_ROOM = 4;
        public const string PLAYER_NAME_PREF_KEY = "PLAYER_NAME";
        public const string PLAYER_NAME_PREF_DEFAULT = "Player";
        public const string NETWORK_PLAYER_TAG = "NetworkPlayer";
        public const string EDITING_SPACE_TAG = "EditingSpace";

        public const string GAME_CONTROLLER_TAG = "GameController";


        /*
         * Photon Custom Event Codes
        */
        public const byte IMPORT_MODEL_FROM_WEB_EVENT_CODE = 1;
        public const byte MESH_VERTEX_PULL_EVENT_CODE = 2;
        public const byte MESH_EDGE_PULL_EVENT_CODE = 3;
        public const byte CLEAR_CANVAS_EVENT_CODE = 4;
        public const byte MESH_FACE_PULL_EVENT_CODE = 5;
        public const byte MESH_FACE_EXTRUDE_EVENT_CODE = 6;
    }
}

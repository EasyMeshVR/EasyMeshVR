using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EasyMeshVR.Core;
using UnityEngine.Networking;
using Parabox.Stl;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace EasyMeshVR.Multiplayer
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkMeshManager : MonoBehaviour, IOnEventCallback
    {
        #region Public Fields

        public static NetworkMeshManager instance { get; private set; }

        public bool isImportingMesh { get; private set; } = false;

        [SerializeField]
        public List<MeshRebuilder> meshRebuilders;

        #endregion

        #region Private Fields

        private PhotonView photonView;
        private Action<bool, string, string> importCallback = null;
        private Queue<NetworkEvent> networkEventQueue;


        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
            networkEventQueue = new Queue<NetworkEvent>();

            if (meshRebuilders == null)
            {
                meshRebuilders = new List<MeshRebuilder>();
            }
        }

        void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region Model Import Callback

        public async void DownloadCallback(DownloadHandler downloadHandler, string error, string modelCode)
        {
            if (!string.IsNullOrEmpty(error))
            {
                isImportingMesh = false;
                if (importCallback != null)
                {
                    importCallback.Invoke(false, error, modelCode);
                    SetImportModelCallback(null);
                }
                return;
            }

            Debug.Log("Importing model into scene...");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Mesh[] meshes = await Importer.Import(downloadHandler.data);

            // Local instantiation of game objects with the imported meshes
            if (meshes == null || meshes.Length < 1)
            {
                isImportingMesh = false;
                Debug.LogError("Meshes array is null or empty");

                if (importCallback != null)
                {
                    importCallback.Invoke(false, "Imported STL mesh was invalid.", modelCode);
                    SetImportModelCallback(null);
                }
                return;
            }

            ModelImportExport.instance.DestroyMeshObjects();
            ModelImportExport.instance.CreateMeshObjects(meshes);

            watch.Stop();
            Debug.LogFormat("Importing model took {0} ms", watch.ElapsedMilliseconds);

            isImportingMesh = false;

            if (importCallback != null)
            {
                importCallback.Invoke(true, "", modelCode);
                SetImportModelCallback(null);
            }

            ProcessNetworkEventQueue();
        }

        #endregion

        #region Public Methods

        public void ClearHeldDataForPlayer(Player otherPlayer)
        {
            foreach (MeshRebuilder meshRebuilder in meshRebuilders)
            {
                if (meshRebuilder == null)
                {
                    Debug.LogWarning("NetworkMeshManager:ClearHeldDataForPlayer() - MeshRebuilder was null in the meshRebuilders list");
                    continue;
                }

                meshRebuilder.ClearHeldDataForPlayer(otherPlayer);
            }
        }

        public void SetImportModelCallback(Action<bool, string, string> callback)
        {
            importCallback = callback;
        }

        public void SynchronizeMeshImport(string modelCode, Action<bool, string, string> callback = null)
        {
            SetImportModelCallback(callback);

            RemoveAllCachedEvents();

            // We tell all other clients to import the model from the web server given the model code.
            // EventCaching.AddToRoomCacheGlobal caches the event globally so that it persists until the room is closed (all players leave),
            // so that new players can import the current model in the scene.
            RaiseEventOptions importModelEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCacheGlobal,
            };

            PhotonNetwork.RaiseEvent(Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE, modelCode, importModelEventOptions, SendOptions.SendReliable);
        }

        public void SynchronizeClearCanvas()
        {
            RemoveAllCachedEvents();

            RaiseEventOptions clearCanvasEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All,
                CachingOption = EventCaching.DoNotCache
            };

            PhotonNetwork.RaiseEvent(Constants.CLEAR_CANVAS_EVENT_CODE, null, clearCanvasEventOptions, SendOptions.SendReliable);
        }

        public void SynchronizeMeshVertexPull(VertexPullEvent vertexEvent)
        {
            EventCaching cachingOption = (vertexEvent.isCached) ? EventCaching.AddToRoomCacheGlobal : EventCaching.DoNotCache;
            RaiseEventOptions meshVertexPullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = cachingOption
            };

            object[] content = VertexPullEvent.SerializeEvent(vertexEvent);
            PhotonNetwork.RaiseEvent(Constants.MESH_VERTEX_PULL_EVENT_CODE, content, meshVertexPullEventOptions, SendOptions.SendReliable);
        }

        public void SynchronizeMeshEdgePull(EdgePullEvent edgeEvent)
        {
            EventCaching cachingOption = (edgeEvent.isCached) ? EventCaching.AddToRoomCacheGlobal : EventCaching.DoNotCache;

            RaiseEventOptions meshEdgePullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = cachingOption
            };

            object[] content = EdgePullEvent.SerializeEvent(edgeEvent);
            PhotonNetwork.RaiseEvent(Constants.MESH_EDGE_PULL_EVENT_CODE, content, meshEdgePullEventOptions, SendOptions.SendReliable);
        }

        public void SynchronizeMeshFacePull(FacePullEvent faceEvent)
        {
            EventCaching cachingOption = (faceEvent.isCached) ? EventCaching.AddToRoomCacheGlobal : EventCaching.DoNotCache;

            RaiseEventOptions meshFacePullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = cachingOption
            };

            object[] content = FacePullEvent.SerializeEvent(faceEvent);
            PhotonNetwork.RaiseEvent(Constants.MESH_FACE_PULL_EVENT_CODE, content, meshFacePullEventOptions, SendOptions.SendReliable);
        }

        public void RemoveCachedEvent(byte eventCode, ReceiverGroup receiverGroup, object eventContent = null)
        {
            RaiseEventOptions removeCachedEventOptions = new RaiseEventOptions
            {
                Receivers = receiverGroup,
                CachingOption = EventCaching.RemoveFromRoomCache
            };

            PhotonNetwork.RaiseEvent(eventCode, eventContent, removeCachedEventOptions, SendOptions.SendReliable);
        }

        public void RemoveCachedEditEvents()
        {
            // This function just removes mesh edit events
            RemoveCachedEvent(Constants.MESH_VERTEX_PULL_EVENT_CODE, ReceiverGroup.Others);
            RemoveCachedEvent(Constants.MESH_EDGE_PULL_EVENT_CODE, ReceiverGroup.Others);
            RemoveCachedEvent(Constants.MESH_FACE_PULL_EVENT_CODE, ReceiverGroup.Others);
        }

        public void RemoveAllCachedEvents()
        {
            // We clear the previous buffered events for importing a model and any cached edits,
            // so that newly joining players aren't importing older models and messing with null data.
            RemoveCachedEvent(Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE, ReceiverGroup.Others);
            RemoveCachedEditEvents();
        }

        #endregion

        #region Pun Callbacks

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            switch (eventCode)
            {
                case Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE:
                    {
                        Debug.Log("Importing model from web...");
                        if (photonEvent.CustomData != null)
                        {
                            string modelCode = (string)photonEvent.CustomData;
                            isImportingMesh = true;
                            ModelImportExport.instance.ImportModel(modelCode, DownloadCallback);
                        }
                        break;
                    }
                case Constants.CLEAR_CANVAS_EVENT_CODE:
                    {
                        ModelImportExport.instance.ClearCanvas();
                        break;
                    }
                case Constants.MESH_VERTEX_PULL_EVENT_CODE:
                    {
                        if (photonEvent.CustomData != null)
                        {
                            object[] data = (object[])photonEvent.CustomData;
                            HandleMeshVertexPullEvent(data);
                        }
                        break;
                    }
                case Constants.MESH_EDGE_PULL_EVENT_CODE:
                    {
                        if (photonEvent.CustomData != null)
                        {
                            object[] data = (object[])photonEvent.CustomData;
                            HandleMeshEdgePullEvent(data);
                        }
                        break;
                    }
                case Constants.MESH_FACE_PULL_EVENT_CODE:
                    {
                        if (photonEvent.CustomData != null)
                        {
                            object[] data = (object[])photonEvent.CustomData;
                            HandleMeshFacePullEvent(data);
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public void ProcessNetworkEventQueue()
        {
            Debug.Log("NetworkMeshManager:ProcessNetworkEventQueue() - Processing network event queue of size: " + networkEventQueue.Count);

            while (networkEventQueue.Count > 0)
            {
                NetworkEvent networkEvent = networkEventQueue.Dequeue();
                System.Type eventType = networkEvent.GetType();

                if (eventType == typeof(VertexPullEvent))
                {
                    HandleMeshVertexPullEvent((VertexPullEvent)networkEvent);
                }
                else if (eventType == typeof(EdgePullEvent))
                {
                    HandleMeshEdgePullEvent((EdgePullEvent)networkEvent);
                }
                else if (eventType == typeof(FacePullEvent))
                {
                    HandleMeshFacePullEvent((FacePullEvent)networkEvent);
                }
            }
        }

        private void HandleMeshVertexPullEvent(object[] data)
        {
            VertexPullEvent vertexEvent = VertexPullEvent.DeserializeEvent(data);

            // Put the vertexEvent in the queue and process it after the NetworkMeshManager is done importing the mesh into the scene
            if (isImportingMesh)
            {
                networkEventQueue.Enqueue(vertexEvent);
            }
            else
            {
                HandleMeshVertexPullEvent(vertexEvent);
            }
        }

        private void HandleMeshVertexPullEvent(VertexPullEvent vertexEvent)
        {
            MeshRebuilder meshRebuilder = meshRebuilders[vertexEvent.meshId];

            if (meshRebuilder == null)
            {
                Debug.LogWarningFormat("NetworkMeshManager:HandleMeshVertexPullEvent() - meshRebuilder is null for meshId {0}", vertexEvent.meshId);
                return;
            }

            Vector3 vertexPos = vertexEvent.vertexPos;
            int index = vertexEvent.id;
            bool released = vertexEvent.released;
            Vertex vertexObj = meshRebuilder.vertexObjects[index];
            MoveVertices moveVertices = vertexObj.GetComponent<MoveVertices>();

            vertexObj.transform.localPosition = vertexPos;
            vertexObj.isHeldByOther = !released;
            vertexObj.heldByActorNumber = (released) ? -1 : vertexEvent.actorNumber;
            meshRebuilder.vertices[index] = vertexPos;
            moveVertices.UpdateMesh(index);
        }

        private void HandleMeshEdgePullEvent(object[] data)
        {
            EdgePullEvent edgeEvent = EdgePullEvent.DeserializeEvent(data);

            // Put the edgeEvent in the queue and process it after the NetworkMeshManager is done importing the mesh into the scene
            if (isImportingMesh)
            {
                networkEventQueue.Enqueue(edgeEvent);
            }
            else
            {
                HandleMeshEdgePullEvent(edgeEvent);
            }
        }

        private void HandleMeshEdgePullEvent(EdgePullEvent edgeEvent)
        {
            MeshRebuilder meshRebuilder = meshRebuilders[edgeEvent.meshId];

            if (meshRebuilder == null)
            {
                Debug.LogWarningFormat("NetworkMeshManager:HandleMeshEdgePullEvent() - meshRebuilder is null for meshId {0}", edgeEvent.meshId);
                return;
            }

            Edge edgeObj = meshRebuilder.edgeObjects[edgeEvent.id];
            Vertex vert1Obj = meshRebuilder.vertexObjects[edgeEvent.vert1];
            Vertex vert2Obj = meshRebuilder.vertexObjects[edgeEvent.vert2];
            MoveEdge moveEdge = edgeObj.GetComponent<MoveEdge>();

            int heldByActorNumber = (edgeEvent.released) ? -1 : edgeEvent.actorNumber;
            edgeObj.isHeldByOther = vert1Obj.isHeldByOther = vert2Obj.isHeldByOther = !edgeEvent.released;
            edgeObj.heldByActorNumber = vert1Obj.heldByActorNumber = vert2Obj.heldByActorNumber = heldByActorNumber;
            vert1Obj.transform.localPosition = edgeEvent.vertex1Pos;
            vert2Obj.transform.localPosition = edgeEvent.vertex2Pos;
            meshRebuilder.vertices[edgeEvent.vert1] = edgeEvent.vertex1Pos;
            meshRebuilder.vertices[edgeEvent.vert2] = edgeEvent.vertex2Pos;
            moveEdge.SetActiveEdges(edgeObj, edgeEvent.released);
            moveEdge.UpdateMesh(edgeEvent.id, edgeEvent.vert1, edgeEvent.vert2, false);
        }

        private void HandleMeshFacePullEvent(object[] data)
        {
            FacePullEvent faceEvent = FacePullEvent.DeserializeEvent(data);

            // Put the edgeEvent in the queue and process it after the NetworkMeshManager is done importing the mesh into the scene
            if (isImportingMesh)
            {
                networkEventQueue.Enqueue(faceEvent);
            }
            else
            {
                HandleMeshFacePullEvent(faceEvent);
            }
        }

        private void HandleMeshFacePullEvent(FacePullEvent faceEvent)
        {
            MeshRebuilder meshRebuilder = meshRebuilders[faceEvent.meshId];

            if (meshRebuilder == null)
            {
                Debug.LogWarningFormat("NetworkMeshManager:HandleMeshFacePullEvent() - meshRebuilder is null for meshId {0}", faceEvent.meshId);
                return;
            }

            Face faceObj = meshRebuilder.faceObjects[faceEvent.id];
            Vertex vert1Obj = meshRebuilder.vertexObjects[faceEvent.vert1];
            Vertex vert2Obj = meshRebuilder.vertexObjects[faceEvent.vert2];
            Vertex vert3Obj = meshRebuilder.vertexObjects[faceEvent.vert3];
            Edge edge1Obj = meshRebuilder.edgeObjects[faceEvent.edge1];
            Edge edge2Obj = meshRebuilder.edgeObjects[faceEvent.edge2];
            Edge edge3Obj = meshRebuilder.edgeObjects[faceEvent.edge3];
            MoveFace moveFace = faceObj.GetComponent<MoveFace>();

            int heldByActorNumber = (faceEvent.released) ? -1 : faceEvent.actorNumber;
            faceObj.isHeldByOther = vert1Obj.isHeldByOther = vert2Obj.isHeldByOther = vert3Obj.isHeldByOther = !faceEvent.released;
            faceObj.heldByActorNumber = vert1Obj.heldByActorNumber = vert2Obj.heldByActorNumber = vert3Obj.heldByActorNumber = heldByActorNumber;
            vert1Obj.transform.localPosition = faceEvent.vertex1Pos;
            vert2Obj.transform.localPosition = faceEvent.vertex2Pos;
            vert3Obj.transform.localPosition = faceEvent.vertex3Pos;
            meshRebuilder.vertices[faceEvent.vert1] = faceEvent.vertex1Pos;
            meshRebuilder.vertices[faceEvent.vert2] = faceEvent.vertex2Pos;
            meshRebuilder.vertices[faceEvent.vert3] = faceEvent.vertex3Pos;
            moveFace.SetActiveEdges(edge1Obj, faceEvent.released);
            moveFace.SetActiveEdges(edge2Obj, faceEvent.released);
            moveFace.SetActiveEdges(edge3Obj, faceEvent.released);
            moveFace.SetActiveFaces(faceObj, faceEvent.released);
            moveFace.UpdateMesh(faceEvent.vert1, faceEvent.vert2, faceEvent.vert3, false);
        }

        #endregion
    }
}

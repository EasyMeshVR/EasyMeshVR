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

        #endregion

        #region Private Fields

        private PhotonView photonView;
        private Action<bool> importCallback = null;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
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

        async void DownloadCallback(DownloadHandler downloadHandler, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when downloading model: {0}", error);

                if (importCallback != null)
                {
                    importCallback.Invoke(false);
                }
                return;
            }

            Debug.Log("Importing model into scene...");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Mesh[] meshes = await Importer.Import(downloadHandler.data);

            // Local instantiation of game objects with the imported meshes
            if (meshes == null || meshes.Length < 1)
            {
                Debug.LogError("Meshes array is null or empty");

                if (importCallback != null)
                {
                    importCallback.Invoke(false);
                }
                return;
            }

            ModelImportExport.instance.DestroyMeshObjects();
            ModelImportExport.instance.CreateMeshObjects(meshes);

            watch.Stop();
            Debug.LogFormat("Importing model took {0} ms", watch.ElapsedMilliseconds);

            if (importCallback != null)
            {
                importCallback.Invoke(true);
            }
        }

        #endregion

        #region Public Methods

        public void SynchronizeMeshImport(string modelCode, Action<bool> callback = null)
        {
            importCallback = callback;

            // We clear the previous buffered event for importing a model so that newly joining
            // players are not importing older models.
            RaiseEventOptions removeImportModelEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All,
                CachingOption = EventCaching.RemoveFromRoomCache
            };

            RaiseEventOptions removeMeshVertexPullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.RemoveFromRoomCache
            };

            RaiseEventOptions removeMeshEdgePullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.RemoveFromRoomCache
            };

            PhotonNetwork.RaiseEvent(Constants.MESH_VERTEX_PULL_EVENT_CODE, null, removeMeshVertexPullEventOptions, SendOptions.SendReliable);
            PhotonNetwork.RaiseEvent(Constants.MESH_EDGE_PULL_EVENT_CODE, null, removeMeshEdgePullEventOptions, SendOptions.SendReliable);
            PhotonNetwork.RaiseEvent(Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE, null, removeImportModelEventOptions, SendOptions.SendReliable);

            // We tell all clients to import the model from the web server given the model code.
            // EventCaching.AddToRoomCacheGlobal caches the event globally so that it persists until the room is closed (all players leave),
            // so that new players can import the current model in the scene.
            RaiseEventOptions importModelEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All,
                CachingOption = EventCaching.AddToRoomCacheGlobal
            };

            object[] content = new object[] { modelCode };

            PhotonNetwork.RaiseEvent(Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE, content, importModelEventOptions, SendOptions.SendReliable);
        }

        public void SynchronizeMeshVertexPull(Vector3 vertex, int index, bool isCached = false, bool released = false)
        {
            EventCaching cachingOption = (isCached) ? EventCaching.AddToRoomCacheGlobal : EventCaching.DoNotCache;

            RaiseEventOptions meshVertexPullEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = cachingOption
            };

            object[] content = new object[] { vertex, index, released };
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

        #endregion

        #region Pun Callbacks

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            switch (eventCode)
            {
                case Constants.IMPORT_MODEL_FROM_WEB_EVENT_CODE:
                    Debug.Log("Importing model from web...");
                    if (photonEvent.CustomData != null)
                    {
                        object[] data = (object[])photonEvent.CustomData;
                        string modelCode = (string)data[0];
                        ModelImportExport.instance.ImportModel(modelCode, DownloadCallback);
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
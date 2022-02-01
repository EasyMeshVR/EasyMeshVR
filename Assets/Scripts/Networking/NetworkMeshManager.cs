using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EasyMeshVR.Core;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Parabox.Stl;

namespace EasyMeshVR.Multiplayer
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkMeshManager : MonoBehaviour
    {
        #region Public Fields

        public static NetworkMeshManager instance { get; private set; }

        #endregion

        #region Private Fields

        // TODO: DEBUGGING delete later: For testing model import
        [SerializeField]
        private InputActionReference importModelInputActionRef;

        private PhotonView photonView;
        private Action<bool> importCallback = null;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
            importModelInputActionRef.action.started += TestImportModelCallback; // TODO: DEBUGGING delete later
        }

        // TODO: DEBUGGING cloud import by using a test input action
        void OnDestroy()
        {
            importModelInputActionRef.action.started -= TestImportModelCallback;
        }

        // TODO: DEBUGGING delete later
        void TestImportModelCallback(InputAction.CallbackContext context)
        {
            // 6 MB
            //SynchronizeMeshImport("494906");

            // 80 KB
            SynchronizeMeshImport("moccasin-tremendous-shark");
        }

        void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        #endregion

        #region RPCs

        [PunRPC]
        void ImportModelFromWeb(string modelCode)
        {
            ModelImportExport.instance.ImportModel(modelCode, DownloadCallback);
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

            // We tell all clients to import the model from the web server given the model code.
            // RpcTarget.AllBufferedViaServer makes it so every player (including the one calling this function)
            // executes the ImportModelFromWeb RPC and it's buffered by the Photon server so that any future
            // players that join can import the model themselves.

            // NOTE: we will need to account for the case where we import a model then delete it and import a new one,
            // we would have to clear the previous buffered RPCs that were sent by NetworkMeshManager (assuming deletion also happens here)
            // whenever we import a new model so new players won't have to import/delete old models for nothing
            // (we can use PhotonNetwork.RemoveRPCs(photonView))
            photonView.RPC("ImportModelFromWeb", RpcTarget.AllBufferedViaServer, modelCode);
        }

        #endregion
    }
}

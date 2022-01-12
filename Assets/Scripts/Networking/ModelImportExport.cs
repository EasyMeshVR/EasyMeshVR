using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using EasyMeshVR.Web;
using Parabox.Stl;
using Photon.Pun;
using EasyMeshVR.Multiplayer;

namespace EasyMeshVR.Core
{
    public class ModelImportExport : MonoBehaviour
    {
        #region Public Fields

        public static ModelImportExport instance;

        #endregion

        #region Private Fields

        [SerializeField]
        private InputActionReference importModelInputActionRef;

        private ApiRequester apiRequester;

        #endregion

        #region Private Constructor

        private ModelImportExport()
        {

        }

        #endregion

        #region ApiRequester Callbacks

        async void DownloadCallback(DownloadHandler downloadHandler, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when downloading model: {0}", error);
                return;
            }

            Debug.Log("Importing model into scene...");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Mesh[] meshes = await Importer.Import(downloadHandler.data);

            // Synchronize the mesh imports by sending RPCs
            NetworkMeshManager.instance.SynchronizeMeshImport(meshes);

            /*if (meshes == null || meshes.Length < 1)
            {
                Debug.LogError("Meshes array is null or empty");
                return;
            }

            for (int i = 0; i < meshes.Length; ++i)
            {
                GameObject go = PhotonNetwork.Instantiate(meshObjectName, Vector3.zero, Quaternion.identity);
                go.name = go.name + "(" + i + ")";

                Mesh mesh = meshes[i];
                mesh.name = "Mesh-" + name + "(" + i + ")";
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
            }*/

            watch.Stop();
            Debug.LogFormat("Importing model took {0} ms", watch.ElapsedMilliseconds);

            // Uncomment to debug cloud export
            // ExportModel(meshes, true);
        }

        void UploadCallback(string modelCode, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when uploading model: {0}", error);
                return;
            }

            Debug.LogFormat("Successfully uploaded model, your model code is {0}", modelCode);
        }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
            importModelInputActionRef.action.started += TestImportModelCallback;
            Importer.InitializeThreadParameters();
            Exporter.InitializeThreadParameters();
        }

        // TODO: DEBUGGING delete later
        void OnDestroy()
        {
            importModelInputActionRef.action.started -= TestImportModelCallback;
        }

        // TODO: DEBUGGING delete later
        void TestImportModelCallback(InputAction.CallbackContext context)
        {
            // 6 MB
            //ImportModel("black-cheerful-roadrunner");

            // 80 KB
            ImportModel("copper-retired-wolf");
        }

        // Start is called before the first frame update
        void Start()
        {
            apiRequester = GetComponent<ApiRequester>();

            // Uncomment to debug cloud import
            // ImportModel("black-cheerful-roadrunner");
        }

        void OnDisable()
        {
            Importer.CancelImportThread();
            Exporter.CancelExportThread();
        }

        #endregion

        #region Public Methods

        public void ImportModel(string modelCode)
        {
            apiRequester.DownloadModel(modelCode, DownloadCallback);
        }

        public async void ExportModel(Mesh[] meshes, bool isCloudUpload, Action<string, string> callback = null)
        {
            if (meshes == null)
            {
                Debug.LogWarning("Failed to export model meshes: meshes is null");
                return;
            }

            string stlData = await Exporter.WriteStringAsync(meshes);

            if (isCloudUpload)
            {
                // Cloud upload
                apiRequester.UploadModel(stlData, UploadCallback);
            }
            else
            {
                // Export to file on disk
                // TODO
            }
        }

        #endregion
    }
}

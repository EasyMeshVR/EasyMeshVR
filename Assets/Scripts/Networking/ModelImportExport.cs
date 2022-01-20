using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using EasyMeshVR.Web;
using Parabox.Stl;

namespace EasyMeshVR.Core
{
    public enum ModelCodeType 
    {
        DIGIT, WORD
    }

    public class ModelImportExport : MonoBehaviour
    {
        #region Public Fields

        public static ModelImportExport instance;

        #endregion

        #region Private Fields

        [SerializeField]
        private GameObject meshObjectPrefab;

        [SerializeField]
        private Transform meshObjectInitialTransform;

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

            // Local instantiation of game objects with the imported meshes
            if (meshes == null || meshes.Length < 1)
            {
                Debug.LogError("Meshes array is null or empty");
                return;
            }

            GameObject parent = new GameObject("Model");
            parent.transform.position = meshObjectInitialTransform.position;
            parent.transform.rotation = meshObjectInitialTransform.rotation;

            for (int i = 0; i < meshes.Length; ++i)
            {
                GameObject go = Instantiate(meshObjectPrefab);
                go.transform.SetParent(parent.transform, false);
                go.name = go.name + "(" + i + ")";

                Mesh mesh = meshes[i];
                mesh.name = "Mesh-" + name + "(" + i + ")";
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            watch.Stop();
            Debug.LogFormat("Importing model took {0} ms", watch.ElapsedMilliseconds);

            // Uncomment to debug cloud export
            //ExportModel(meshes, true, ModelCodeType.WORD);
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
            Importer.InitializeThreadParameters();
            Exporter.InitializeThreadParameters();
        }

        void Start()
        {
            apiRequester = GetComponent<ApiRequester>();
        }

        void OnDisable()
        {
            Importer.CancelImportThread();
            Exporter.CancelExportThread();
        }

        #endregion

        #region Public Methods

        public void ImportModel(string modelCode, Action<DownloadHandler, string> callback = null)
        {
            apiRequester.DownloadModel(modelCode, DownloadCallback);
        }

        public async void ExportModel(Mesh[] meshes, bool isCloudUpload, ModelCodeType modelCodeType, Action<string, string> callback = null)
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
                apiRequester.UploadModel(stlData, modelCodeType.ToString(), UploadCallback);
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

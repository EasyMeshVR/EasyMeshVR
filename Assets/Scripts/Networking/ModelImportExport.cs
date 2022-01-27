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

        public static ModelImportExport instance { get; private set; }

        #endregion

        #region Private Fields

        [SerializeField]
        private GameObject meshObjectPrefab;

        [SerializeField]
        private GameObject modelObject;

        private ApiRequester apiRequester;

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

            /*GameObject parent = new GameObject("Model");
            parent.transform.position = meshObjectInitialTransform.position;
            parent.transform.rotation = meshObjectInitialTransform.rotation;
*/
            for (int i = 0; i < meshes.Length; ++i)
            {
                GameObject go = Instantiate(meshObjectPrefab);
                go.transform.SetParent(modelObject.transform, false);
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

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
            Importer.InitializeThreadParameters();
            Exporter.InitializeThreadParameters();
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

        public async void ExportModel(bool isCloudUpload, ModelCodeType modelCodeType, Action<string, string> callback = null)
        {
            MeshFilter[] meshFilters = modelObject.GetComponentsInChildren<MeshFilter>();

            if (meshFilters == null || meshFilters.Length == 0)
            {
                Debug.LogWarning("Failed to export model meshes: meshes is null");
                callback.Invoke(null, "No Mesh Found");
                return;
            }   
            
            Mesh[] meshes = new Mesh[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; ++i)
            {
                meshes[i] = meshFilters[i].sharedMesh;
            }

            string stlData = await Exporter.WriteStringAsync(meshes);

            if (isCloudUpload)
            {
                // Cloud upload
                apiRequester.UploadModel(stlData, modelCodeType.ToString(), callback);
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

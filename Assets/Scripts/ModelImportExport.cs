using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EasyMeshVR.Web;
using Parabox.Stl;
using Photon.Pun;

namespace EasyMeshVR.Core
{
    public class ModelImportExport : MonoBehaviour
    {
        #region Public Fields

        public static ModelImportExport instance;

        #endregion

        #region Private Fields

        [SerializeField]
        private string modelObjectName = "Model";

        [SerializeField]
        private Transform modelObjectInitialTransform;

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

            if (meshes == null)
            {
                return;
            }

            if (meshes.Length < 1)
                return;

            var parent = PhotonNetwork.Instantiate(modelObjectName, modelObjectInitialTransform.position, modelObjectInitialTransform.rotation);

            if (meshes.Length < 2)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(go.GetComponent<BoxCollider>());
                go.transform.SetParent(parent.transform, false);
                go.name = name;
                meshes[0].name = "Mesh-" + name;
                go.GetComponent<MeshFilter>().sharedMesh = meshes[0];
            }
            else
            {
                for (int i = 0, c = meshes.Length; i < c; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(go.GetComponent<BoxCollider>());
                    go.transform.SetParent(parent.transform, false);
                    go.name = name + "(" + i + ")";

                    var mesh = meshes[i];
                    mesh.name = "Mesh-" + name + "(" + i + ")";
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                }
            }

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
            Importer.InitializeThreadParameters();
            Exporter.InitializeThreadParameters();
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

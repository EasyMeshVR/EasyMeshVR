using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EasyMeshVR.Web;
using Parabox.Stl;

namespace EasyMeshVR.Core
{
    public class ModelImportExport : MonoBehaviour
    {
        #region Public Fields

        public static ModelImportExport instance;

        #endregion

        #region Private Fields

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

            watch.Stop();
            Debug.LogFormat("Importing model took {0} ms", watch.ElapsedMilliseconds);

            if (meshes.Length < 1)
                return;

            var parent = gameObject;

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
        }

        void UploadCallback()
        {
            // TODO
        }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            apiRequester = GetComponent<ApiRequester>();

            // TODO: remove later (debugging)
            apiRequester.DownloadModel("chocolate-related-monkey", DownloadCallback);
            //apiRequester.DownloadModel("gold-preliminary-smelt", DownloadCallback);
        }

        #endregion

        #region Public Methods

        public void ImportModel(string modelCode)
        {
            apiRequester.DownloadModel(modelCode, DownloadCallback);
        }

        public void ExportModel()
        {
            // TODO
        }

        #endregion
    }
}

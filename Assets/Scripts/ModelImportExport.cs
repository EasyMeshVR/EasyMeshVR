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

        void DownloadCallback(DownloadHandler downloadHandler, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("Error encountered when downloading model: {0}", error);
                return;
            }

            // TODO: currently Import function is blocking the game until it's finished which lags which higher poly models
            // need to find a way to make it async or run in a separate job/thread
            Mesh[] meshes = Importer.Import(downloadHandler.data);

            if (meshes.Length < 1)
                return;

            var parent = new GameObject();
            parent.name = name;

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

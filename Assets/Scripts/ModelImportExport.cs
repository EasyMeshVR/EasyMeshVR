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

            Debug.Log(downloadHandler.text);

            // TODO: import the model into the scene using the STL parser library
            //Importer.Import("test");
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

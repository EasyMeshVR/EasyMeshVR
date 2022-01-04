using System;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyMeshVR.Web
{
    public class ApiRequester : MonoBehaviour
    {
        #region JSON Classes

        [Serializable]
        public class PresignedGetJSON
        {
            public string url;
        }

        [Serializable]
        public class PresignedPostJSON
        {
            // TODO
        }

        #endregion

        #region Private Fields

        [SerializeField]
        private string FILE_SERVICE_ENDPOINT = "https://fzq7qh0yub.execute-api.us-east-2.amazonaws.com/file";

        [SerializeField]
        private string NAME_CODE_QUERY_PARAM = "nameCode";

        #endregion

        #region Public Methods

        public void DownloadModel(string modelCode, Action<DownloadHandler, string> callback = null)
        {
            StartCoroutine(RequestPresignedGet(modelCode, callback));
        }

        public void UploadModel()
        {
            // TODO
        }

        #endregion

        #region Private Methods

        private static Uri BuildUri(string url, Dictionary<string, string> queryParams = null)
        {
            UriBuilder uriBuilder = new UriBuilder(url);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    query.Add(param.Key, param.Value);
                }
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }

        private IEnumerator RequestPresignedGet(string modelCode, Action<DownloadHandler, string> callback = null)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>()
            {
                { NAME_CODE_QUERY_PARAM, modelCode }
            };

            Uri uri = BuildUri(FILE_SERVICE_ENDPOINT, queryParams);
            UnityWebRequest webRequest = UnityWebRequest.Get(uri);

            yield return webRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(webRequest.error))
            {
                if (callback != null)
                {
                    callback.Invoke(webRequest.downloadHandler, webRequest.error);
                }
            }
            else
            {
                PresignedGetJSON presignedGetJSON = JsonUtility.FromJson<PresignedGetJSON>(webRequest.downloadHandler.text);
                StartCoroutine(FetchModelData(presignedGetJSON.url, callback));
            }
        }

        private IEnumerator FetchModelData(string presignedGetUrl, Action<DownloadHandler, string> callback = null)
        {
            Uri uri = BuildUri(presignedGetUrl);
            UnityWebRequest webRequest = UnityWebRequest.Get(uri);

            yield return webRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(webRequest.error))
            {
                if (callback != null)
                {
                    callback.Invoke(webRequest.downloadHandler, webRequest.error);
                }
            }
            else if (callback != null)
            {
                callback.Invoke(webRequest.downloadHandler, null);
            }
        }

        private IEnumerator RequestPresignedPost()
        {
            yield return 1;
        }

        #endregion
    }
}

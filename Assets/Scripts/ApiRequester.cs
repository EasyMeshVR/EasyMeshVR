using System;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace EasyMeshVR.Web
{
    public class ApiRequester : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private string FILE_SERVICE_ENDPOINT = "https://fzq7qh0yub.execute-api.us-east-2.amazonaws.com/file";

        private const string NAME_CODE_QUERY_PARAM = "nameCode";
        private const string BUCKET_PARAM = "bucket";
        private const string X_AMZ_ALGORITHM_PARAM = "X-Amz-Algorithm";
        private const string X_AMZ_CREDENTIAL_PARAM = "X-Amz-Credential";
        private const string X_AMZ_DATE_PARAM = "X-Amz-Date";
        private const string X_AMZ_SECURITY_TOKEN_PARAM = "X-Amz-Security-Token";
        private const string X_AMZ_SIGNATURE_PARAM = "X-Amz-Signature";
        private const string POLICY_PARAM = "Policy";
        private const string KEY_PARAM = "key";
        private const string FILE_PARAM = "file";

        #endregion

        #region JSON Classes

        [Serializable]
        public class PresignedGetJSON
        {
            public string url;
        }

        [Serializable]
        public class PresignedPostJSON
        {
            public string nameCode;
            public PresignedPostData data;
        }

        [Serializable]
        public class PresignedPostData
        {
            public string url;
            public PresignedPostFields fields;
        }

        [Serializable]
        public class PresignedPostFields
        {
            public string bucket;
            public string key;
            [JsonProperty(X_AMZ_ALGORITHM_PARAM)]
            public string xAmzAlgorithm;
            [JsonProperty(X_AMZ_CREDENTIAL_PARAM)]
            public string xAmzCredential;
            [JsonProperty(X_AMZ_DATE_PARAM)]
            public string xAmzDate;
            [JsonProperty(X_AMZ_SECURITY_TOKEN_PARAM)]
            public string xAmzSecurityToken;
            [JsonProperty(POLICY_PARAM)]
            public string policy;
            [JsonProperty(X_AMZ_SIGNATURE_PARAM)]
            public string xAmzSignature;
        }

        #endregion

        #region Public Methods

        public void DownloadModel(string modelCode, Action<DownloadHandler, string> callback = null)
        {
            StartCoroutine(RequestPresignedGet(modelCode, callback));
        }

        public void UploadModel(string stlData, Action<string, string> callback = null)
        {
            StartCoroutine(RequestPresignedPost(stlData, callback));
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
                PresignedGetJSON presignedGetJSON = JsonConvert.DeserializeObject<PresignedGetJSON>(webRequest.downloadHandler.text);
                StartCoroutine(FetchModelData(presignedGetJSON.url, callback));
            }
        }

        private IEnumerator FetchModelData(string presignedGetUrl, Action<DownloadHandler, string> callback = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(presignedGetUrl);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
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

        private IEnumerator RequestPresignedPost(string stlData, Action<string, string> callback = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Post(FILE_SERVICE_ENDPOINT, "");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                if (callback != null)
                {
                    callback.Invoke(null, webRequest.error);
                }
            }
            else
            {
                PresignedPostJSON presignedPostJSON = JsonConvert.DeserializeObject<PresignedPostJSON>(webRequest.downloadHandler.text);
                StartCoroutine(UploadModelData(presignedPostJSON, stlData, callback));
            }
        }

        private IEnumerator UploadModelData(PresignedPostJSON presignedPostJSON, string stlData, Action<string, string> callback = null)
        {
            PresignedPostFields fields = presignedPostJSON.data.fields;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection(BUCKET_PARAM, fields.bucket));
            formData.Add(new MultipartFormDataSection(X_AMZ_ALGORITHM_PARAM, fields.xAmzAlgorithm));
            formData.Add(new MultipartFormDataSection(X_AMZ_CREDENTIAL_PARAM, fields.xAmzCredential));
            formData.Add(new MultipartFormDataSection(X_AMZ_DATE_PARAM, fields.xAmzDate));
            formData.Add(new MultipartFormDataSection(X_AMZ_SECURITY_TOKEN_PARAM, fields.xAmzSecurityToken));
            formData.Add(new MultipartFormDataSection(KEY_PARAM, fields.key));
            formData.Add(new MultipartFormDataSection(POLICY_PARAM, fields.policy));
            formData.Add(new MultipartFormDataSection(X_AMZ_SIGNATURE_PARAM, fields.xAmzSignature));
            formData.Add(new MultipartFormDataSection(FILE_PARAM, stlData)); // must be the last form field

            UnityWebRequest webRequest = UnityWebRequest.Post(presignedPostJSON.data.url, formData);
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                if (callback != null)
                {
                    callback.Invoke(null, webRequest.error);
                }
            }
            else if (callback != null)
            {
                callback.Invoke(presignedPostJSON.nameCode, null);
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EasyMeshVR.Web;
using Parabox.Stl;
using EasyMeshVR.Multiplayer;

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

        [SerializeField] 
        private GameObject vertex;

        [SerializeField] 
        private GameObject edge;

        [SerializeField]
        private GameObject face;

        [SerializeField] 
        private GameObject cubePrefab;

        private ApiRequester apiRequester;

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

        public void ImportModel(string modelCode, Action<DownloadHandler, string, string> callback = null)
        {
            apiRequester.DownloadModel(modelCode, callback);
        }

        public async void ExportModel(bool isCloudUpload, ModelCodeType modelCodeType, Action<string, string> callback = null)
        {
            if (modelObject.transform.childCount == 0)
            {
                callback.Invoke(null, "No mesh found");
                return;
            }

            List<Mesh> meshes = new List<Mesh>();

            for (int i = 0; i < modelObject.transform.childCount; ++i)
            {
                MeshFilter mf = modelObject.transform.GetChild(i).GetComponent<MeshFilter>();

                if (!mf || !mf.sharedMesh)
                {
                    continue;
                }

                meshes.Add(mf.sharedMesh);
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

        public void CreateMeshObjects(Mesh[] meshes)
        {
            if (meshes == null || meshes.Length < 1)
            {
                Debug.LogError("Meshes array is null or empty");
                return;
            }

            // Local instantiation of game objects with the imported meshes
            for (int i = 0; i < meshes.Length; ++i)
            {
                GameObject go = Instantiate(meshObjectPrefab);
                go.transform.SetParent(modelObject.transform, false);
                go.name = go.name + "(" + i + ")";

                Mesh mesh = meshes[i];
                mesh.name = "Mesh-" + name + "(" + i + ")";
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                MeshRebuilder rebuilder = go.GetComponent<MeshRebuilder>();
                rebuilder.id = i;
                rebuilder.Initialize();
                rebuilder.enabled = true;
                NetworkMeshManager.instance.meshRebuilders.Add(rebuilder);
            }
        }

        public void DestroyMeshObjects()
        {
            // Clear previous MeshRebuilders stored in NetworkMeshManager
            NetworkMeshManager.instance.meshRebuilders.Clear();

            // This will delete all the mesh-related game objects under the modelObject prefab
            // but we make sure we don't delete the Network Players that were parented in the editing space
            foreach (Transform child in modelObject.transform)
            {
                if(!child.gameObject.CompareTag(Constants.NETWORK_PLAYER_TAG))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void CreateCubeMeshObject()
        {
            GameObject go = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(modelObject.transform, false);
            NetworkMeshManager.instance.meshRebuilders.Add(go.GetComponent<MeshRebuilder>());
        }

        public void ClearCanvas()
        {
            DestroyMeshObjects();
            CreateCubeMeshObject();
            NetworkPlayerManager.instance.radiusGameMenuManager.gameMenu.abilitiesMenuPanel.HandleAbilities();
        }

        #endregion
    }
}

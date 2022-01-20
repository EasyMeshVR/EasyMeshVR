using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EasyMeshVR.Core;
using UnityEngine.InputSystem;

namespace EasyMeshVR.Multiplayer
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkMeshManager : MonoBehaviour
    {
        #region Public Fields

        public static NetworkMeshManager instance;

        public Mesh[] meshes = null;

        #endregion

        #region Private Fields

        [SerializeField]
        private GameObject meshObjectPrefab;

        [SerializeField]
        private Transform meshObjectInitialTransform;

        // For testing model import
        [SerializeField]
        private InputActionReference importModelInputActionRef;

        PhotonView photonView;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
            importModelInputActionRef.action.started += TestImportModelCallback;
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
            SynchronizeMeshImport("494906");

            // 80 KB
            //SynchronizeMeshImport("gold-dominant-heron");
        }

        void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        #endregion

        #region RPCs

        [PunRPC]
        void InstantiateMeshes(Vector3[][] verts, Vector3[][] norms, int[][] tris)
        {
            if (verts == null || norms ==  null || tris == null)
            {
                Debug.LogError("InstantiateMeshes RPC failed because input is null");
                return;
            }

            GameObject parent = new GameObject("Model");
            parent.transform.position = meshObjectInitialTransform.position;
            parent.transform.rotation = meshObjectInitialTransform.rotation;

            for (int i = 0; i < verts.Length; ++i)
            {
                GameObject meshObject = Instantiate(meshObjectPrefab);
                meshObject.transform.SetParent(parent.transform, false);

                meshObject.name += "(" + i + ")";

                Mesh mesh = new Mesh();
                mesh.name = "Mesh-" + name + "(" + i + ")";
                mesh.vertices = verts[i];
                mesh.normals = norms[i];
                mesh.triangles = tris[i];

                meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            Debug.LogFormat("Instantiating {0} meshes", verts.Length);
        }

        [PunRPC]
        void ImportModelFromWeb(string modelCode)
        {
            ModelImportExport.instance.ImportModel(modelCode);
        }

        #endregion

        #region Public Methods

        public void SynchronizeMeshImport(string modelCode)
        {
            // TODO: this does not work with large meshes (tested with a 6MB stl file size)
            // Find a way to send the data more compactly (binary compression?)
            // or spaced out in multiple RPCs (maybe one for each mesh using coroutines that fire off in fixed intervals?)

            /*Vector3[][] verts = new Vector3[meshes.Length][];
            Vector3[][] norms = new Vector3[meshes.Length][];
            int[][] tris = new int[meshes.Length][];

            for (int i = 0; i < meshes.Length; ++i)
            {
                verts[i] = meshes[i].vertices;
                norms[i] = meshes[i].normals;
                tris[i] = meshes[i].triangles;

                Debug.LogFormat("Mesh {0} has {1} verts {2} norms {3} tris", i, verts[i].Length, norms[i].Length, tris[i].Length);
            }

            photonView.RPC("InstantiateMeshes", RpcTarget.AllBufferedViaServer, verts, norms, tris);*/

            // Instead we can tell all other clients to import the model from the web server as well
            photonView.RPC("ImportModelFromWeb", RpcTarget.AllBufferedViaServer, modelCode);
        }

        #endregion
    }
}

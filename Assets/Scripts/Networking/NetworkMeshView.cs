using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace EasyMeshVR.Multiplayer
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NetworkMeshView : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            if (meshFilter == null)
            {
                Debug.Log("Mesh filter is null!!!!");
            }
            if (meshFilter.sharedMesh == null)
            {
                Debug.Log("shared mesh is null, creating a new mesh object");
                meshFilter.sharedMesh = new Mesh();
            }

            Debug.Log("In start:");
            Debug.Log(meshFilter.sharedMesh.vertices.Length);
            Debug.Log(meshFilter.sharedMesh.normals.Length);
            Debug.Log(meshFilter.sharedMesh.triangles.Length);
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (stream == null)
                {
                    Debug.Log("stream is null");
                    return;
                }
                if (meshFilter == null)
                {
                    Debug.Log("mesh filter is null");
                    return;
                }
                if (meshFilter.sharedMesh == null)
                {
                    Debug.Log("meshfilter shared mesh is null");
                    return;
                }

                /*Vector3[] v = meshFilter.sharedMesh.vertices;
                Vector3[] n = meshFilter.sharedMesh.normals;*/
                int[] t = meshFilter.sharedMesh.triangles;

                Debug.Log("Sending network mesh data");
                Debug.Log(t.Length);
                /*Debug.Log(v.Length);
                Debug.Log(n.Length);
                Debug.Log(t.Length);*/

                Debug.Log("Sending network mesh data");

                stream.SendNext(t);
            }
            else
            {
                Debug.Log("Receiving network mesh data");

                /*if (meshFilter.sharedMesh == null)
                {
                    Debug.Log("shared mesh is null inside receiver");
                    return;
                }*/

                /*Vector3[] v = (Vector3[])stream.ReceiveNext();
                Vector3[] n = (Vector3[])stream.ReceiveNext();
                int[] t = (int[])stream.ReceiveNext();

                Debug.Log("Received:");
                Debug.Log(v.Length);
                Debug.Log(n.Length);
                Debug.Log(t.Length);*/

                /*meshFilter.sharedMesh.vertices = (Vector3[])stream.ReceiveNext();
                meshFilter.sharedMesh.normals = (Vector3[])stream.ReceiveNext();
                meshFilter.sharedMesh.triangles = (int[])stream.ReceiveNext();*/

                int[] t = (int[])stream.ReceiveNext();

                Debug.Log("Received:");
                Debug.Log(t.Length);
            }
        }

        #endregion
    }
}

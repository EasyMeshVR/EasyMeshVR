using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace EasyMeshVR.Core
{
    public class SceneLoader : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        public static SceneLoader instance { get; private set; }

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            instance = this;
        }

        #endregion

        #region Pun Callbacks

        public override void OnLeftRoom()
        {
            Debug.Log("The local player has left the room");
            AsyncLoadScene(0);
        }

        #endregion

        #region Private Methods

        private void AsyncLoadScene(int buildIndex)
        {
            StartCoroutine(AsyncLoadSceneCoroutine(buildIndex));
        }

        private IEnumerator AsyncLoadSceneCoroutine(int buildIndex)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        #endregion
    }
}

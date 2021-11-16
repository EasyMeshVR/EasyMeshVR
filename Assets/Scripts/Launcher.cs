using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace EasyMeshVR.Multiplayer
{
    public class Launcher : MonoBehaviour
    {
        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region Public Methods

        public void OnClickedSinglePlayer()
        {
            Debug.Log("Clicked Single Player");
        }

        public void OnClickedMultiPlayer()
        {
            Debug.Log("Clicked Multi Player");
        }

        public void OnClickedQuit()
        {
            Debug.Log("Clicked Quit");
            Application.Quit();
        }

        #endregion
    }
}

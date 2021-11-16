using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public void OnClickedQuit()
        {
            Application.Quit();
        }

        #endregion
    }
}

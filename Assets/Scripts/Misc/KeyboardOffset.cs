using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using VRKeys;

namespace EasyMeshVR.Core
{
    [RequireComponent(typeof(XROrigin))]
    public class KeyboardOffset : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Keyboard keyboard;

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            keyboard.XRRigOffset = transform.position;
        }

        #endregion
    }
}

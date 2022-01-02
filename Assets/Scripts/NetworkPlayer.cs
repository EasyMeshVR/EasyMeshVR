using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;

namespace EasyMeshVR.Multiplayer
{
    public class NetworkPlayer : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        private Transform headOrigin;
        private Transform leftHandOrigin;
        private Transform rightHandOrigin;

        #endregion

        #region Public Fields

        public PhotonView photonView { get; set; }

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            photonView = GetComponent<PhotonView>();

            XROrigin origin = FindObjectOfType<XROrigin>();
            headOrigin = origin.transform.Find("Camera Offset/Main Camera");
            leftHandOrigin = origin.transform.Find("Camera Offset/LeftHand Controller");
            rightHandOrigin = origin.transform.Find("Camera Offset/RightHand Controller");
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                head.GetChild(0).gameObject.SetActive(false);
                leftHand.GetChild(0).gameObject.SetActive(false);
                rightHand.GetChild(0).gameObject.SetActive(false);

                MapPosition(head, headOrigin);
                MapPosition(leftHand, leftHandOrigin);
                MapPosition(rightHand, rightHandOrigin);
            }
        }

        #endregion

        #region Private Methods

        void MapPosition(Transform target, Transform originTransform)
        {
            target.position = originTransform.position;
            target.rotation = originTransform.rotation;
        }

        #endregion
    }
}

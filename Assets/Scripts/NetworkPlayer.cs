using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;
using Photon.Realtime;

namespace EasyMeshVR.Multiplayer
{
    public class NetworkPlayer : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Animator leftHandAnimator;
        [SerializeField] private Animator rightHandAnimator;

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

            if (photonView.IsMine)
            {
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }

                foreach (var canvas in GetComponentsInChildren<Canvas>())
                {
                    canvas.enabled = false;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                MapPosition(head, headOrigin);
                MapPosition(leftHand, leftHandOrigin);
                MapPosition(rightHand, rightHandOrigin);

                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
            }
        }

        #endregion

        #region Private Methods

        void MapPosition(Transform target, Transform originTransform)
        {
            target.position = originTransform.position;
            target.rotation = originTransform.rotation;
        }

        void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
        {
            if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                handAnimator.SetFloat("Trigger", triggerValue);
            }
            else
            {
                handAnimator.SetFloat("Trigger", 0);
            }

            if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            {
                handAnimator.SetFloat("Grip", gripValue);
            }
            else
            {
                handAnimator.SetFloat("Grip", 0);
            }
        }

        #endregion
    }
}

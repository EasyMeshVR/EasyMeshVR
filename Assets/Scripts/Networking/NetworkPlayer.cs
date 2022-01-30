using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;
using TMPro;

namespace EasyMeshVR.Multiplayer
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkPlayer : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Animator leftHandAnimator;
        [SerializeField] private Animator rightHandAnimator;
        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private TMP_Text playerNameText;

        private Transform headOrigin;
        private Transform leftHandOrigin;
        private Transform rightHandOrigin;

        private PhotonView photonView;

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

            // Set player's name text
            string playerName = (string)photonView.InstantiationData[0];
            playerNameText.text = playerName;

            if (photonView.IsMine)
            {
                // Disabling Renderers for the local player's avatar
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }

                // Disable Canvas of the player's name above his head
                playerNameCanvas.enabled = false;
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

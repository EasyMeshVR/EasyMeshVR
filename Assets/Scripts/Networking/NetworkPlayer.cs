using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;
using TMPro;
using EasyMeshVR.Core;

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
        private Transform mainCameraTransform;
        private PhotonView photonView;
        private GameObject editingSpace;
        
        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            mainCameraTransform = Camera.main.transform;

            // TODO: FIX BUG, head and left/right hand transforms are slightly off
            XROrigin origin = FindObjectOfType<XROrigin>();
            headOrigin = origin.transform.Find("Camera Offset/Main Camera");
            leftHandOrigin = origin.transform.Find("Camera Offset/LeftRadius");
            rightHandOrigin = origin.transform.Find("Camera Offset/RightRadius");

            /*headOrigin = NetworkPlayerManager.instance.head;
            leftHandOrigin = SwitchControllers.instance.activeLeftControllerTransform;
            rightHandOrigin = SwitchControllers.instance.activeRightControllerTransform;*/

            editingSpace = GameObject.FindGameObjectWithTag(Constants.EDITING_SPACE_TAG);

            // Initialize the transform of other joining players based on our local editing space transform.
            if (!photonView.IsMine)
            {
                InitTransform(transform, editingSpace.transform);
            }

            // This makes it so that players' transforms stay in position
            // relative to our local editing space.
            transform.parent = editingSpace.transform;

            // Set player's name text
            playerNameText.text = photonView.Owner.NickName;

            if (photonView.IsMine)
            {
                // Disabling Renderers for the local player's avatar
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }

                // Disable Canvas of the player's name above the head of the local player
                playerNameCanvas.enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                /*leftHandOrigin = SwitchControllers.instance.activeLeftControllerTransform;
                rightHandOrigin = SwitchControllers.instance.activeRightControllerTransform;*/

                MapPosition(head, headOrigin);
                MapPosition(leftHand, leftHandOrigin);
                MapPosition(rightHand, rightHandOrigin);

                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
            }
        }

        void LateUpdate()
        {
            playerNameCanvas.transform.LookAt(
                playerNameCanvas.transform.position + mainCameraTransform.rotation * Vector3.forward);
        }

        #endregion

        #region Private Methods

        void MapPosition(Transform target, Transform originTransform)
        {
            target.position = originTransform.position;
            target.rotation = originTransform.rotation;

            // Calculate inverse scale vector
            Vector3 editingSpaceScale = editingSpace.transform.lossyScale;
            if (editingSpaceScale.x != 0 && editingSpaceScale.y != 0 && editingSpaceScale.z != 0)
            {
                Vector3 inverseScale = new Vector3(
                    1.0f / editingSpaceScale.x,
                    1.0f / editingSpaceScale.y,
                    1.0f / editingSpaceScale.z
                );
                target.localScale = inverseScale;
            }
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

        void InitTransform(Transform target, Transform originTransform)
        {
            target.transform.position = target.transform.position + originTransform.transform.position;
            target.transform.rotation = target.transform.rotation * originTransform.transform.rotation;
            target.transform.localScale = Vector3.Scale(target.transform.lossyScale, originTransform.transform.lossyScale);
        }

        #endregion
    }
}

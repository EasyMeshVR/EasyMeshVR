using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using Photon.Pun;
using TMPro;
using EasyMeshVR.Core;
using EasyMeshVR.UI;

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
        [SerializeField] private AudioSource micAudioSource;

        private Transform headOrigin;
        private Transform leftHandOrigin;
        private Transform rightHandOrigin;
        private Transform mainCameraTransform;
        private GameObject editingSpace;
        public PhotonView photonView { get; private set; }

        #endregion

        #region MonoBehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            mainCameraTransform = Camera.main.transform;

            headOrigin = mainCameraTransform;
            leftHandOrigin = SwitchControllers.instance.activeLeftController.transform;
            rightHandOrigin = SwitchControllers.instance.activeRightController.transform;

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

            PlayerPrefsInit();

            // Finally add this NetworkPlayer to the NetworkPlayerManager dictionary
            NetworkPlayerManager.instance.AddNetworkPlayer(this);
        }

        // Update is called once per frame
        void Update()
        {
            // Update the player's name in the case that it has changed
            if (playerNameText.text != photonView.Owner.NickName)
            {
                playerNameText.text = photonView.Owner.NickName;
            }

            if (photonView.IsMine)
            {
                leftHandOrigin = SwitchControllers.instance.activeLeftController.transform;
                rightHandOrigin = SwitchControllers.instance.activeRightController.transform;

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

        #region Public Methods

        public void SetPlayerNameVisible(bool visible)
        {
            playerNameCanvas.enabled = visible;
        }

        public void SetMuteMic(bool muted)
        {
            micAudioSource.mute = muted;
        }

        public void ToggleMuteMic()
        {
            micAudioSource.mute = !micAudioSource.mute;
        }

        public void PlayerPrefsInit()
        {
            // Disable playerNameCanvas if our player prefs for hiding player names is true
            if (PlayerPrefs.GetInt(Constants.HIDE_PLAYER_NAMES_PREF_KEY) != 0)
            {
                playerNameCanvas.enabled = false;
            }
            // Disable microphone if this NetworkPlayer belongs to us and our pref is set to true
            if (photonView.IsMine && PlayerPrefs.GetInt(Constants.MUTE_MIC_ON_JOIN_PREF_KEY) != 0)
            {
                micAudioSource.mute = true;
            }
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

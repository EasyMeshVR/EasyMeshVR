using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace EasyMeshVR.Core
{
    public class HandPresence : MonoBehaviour
    {
        public bool showController = false;
        public InputDeviceCharacteristics controllerCharacteristics;
        public List<GameObject> controllerPrefabs;
        public GameObject handModelPrefab;

        private InputDevice targetDevice;
        private GameObject spawnedController;
        public GameObject spawnedHandModel { get; private set; }
        private Animator handAnimator;

        public bool initialized = false;
        public bool initializing = false;

        private void Awake()
        {
            initializing = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!initialized && !initializing)
            {
                TryInitialize();
            }
        }

        public void TryInitialize()
        {
            List<InputDevice> devices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

            foreach (var item in devices)
            {
                Debug.Log(item.name + item.characteristics);
            }

            if (devices.Count > 0)
            {
                targetDevice = devices[0];
                GameObject prefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name);
                if (prefab)
                {
                    spawnedController = Instantiate(prefab, transform);
                }

                spawnedHandModel = Instantiate(handModelPrefab, transform);
                handAnimator = spawnedHandModel.GetComponent<Animator>();
            }

            initialized = true;
            initializing = false;
        }

        void UpdateHandAnimation()
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

        // Update is called once per frame
        void Update()
        {
            if (!targetDevice.isValid)
            {
                TryInitialize();
            }
            else
            {
                if (showController)
                {
                    if (spawnedHandModel)
                        spawnedHandModel.SetActive(false);
                    if (spawnedController)
                        spawnedController.SetActive(true);
                }
                else
                {
                    if (spawnedHandModel)
                        spawnedHandModel.SetActive(true);
                    if (spawnedController)
                        spawnedController.SetActive(false);
                    UpdateHandAnimation();
                }
            }
        }
    }
}

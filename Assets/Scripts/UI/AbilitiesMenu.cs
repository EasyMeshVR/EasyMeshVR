using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMeshVR.Core;

namespace EasyMeshVR.UI
{
    public class AbilitiesMenu : MonoBehaviour
    {
        #region Private Fields

        private static Color untoggledButtonColor = new Color(255, 255, 255, 150);
        private static Color toggledButtonColor = new Color(255, 255, 255, 255);

        private bool extrudeToolToggled;
        private bool lockToolToggled;

        [SerializeField] private Toggle grabVerticesToggle;
        [SerializeField] private Toggle grabEdgesToggle;
        [SerializeField] private Toggle grabFacesToggle;
        [SerializeField] private Toggle autoMergeVerticesToggle;
        [SerializeField] private Toggle raycastGrabToggle;

        [SerializeField] private Button extrudeToolButton;
        [SerializeField] private Button lockToolButton;
        [SerializeField] private Button resetDefaultsToggle;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            extrudeToolToggled = false;
            lockToolToggled = false;

            extrudeToolButton.image.color = untoggledButtonColor;
            lockToolButton.image.color = untoggledButtonColor;

            SetDefaultAbilities();
        }

        #endregion

        #region Public Methods

        public void HandleGrabVertices(bool toggled)
        {
            if (toggled)
            {
                ToolManager.instance.EnableVertex();
            }
            else
            {
                ToolManager.instance.DisableVertex();
            }
        }

        public void HandleGrabEdges(bool toggled)
        {
            if (toggled)
            {
                ToolManager.instance.EnableEdge();
            }
            else
            {
                ToolManager.instance.DisableEdge();
            }
        }

        public void HandleGrabFaces(bool toggled)
        {
            if (toggled)
            {
                ToolManager.instance.EnableFace();
            }
            else
            {
                ToolManager.instance.DisableFace();
            }
        }

        // TODO: when merge ability is done finish this function
        public void HandleAutoMergeVertices(bool toggled)
        {
            if (toggled)
            {

            }
            else
            {

            }
        }

        public void HandleRaycastGrab(bool toggled)
        {
            if (toggled)
            {
                SwitchControllers.instance.switchToRay();
            }
            else
            {
                SwitchControllers.instance.switchToGrab();
            }
        }

        public void HandleExtrudeTool()
        {
            if (!extrudeToolToggled)
            {
                DisableAllTools();
                ToolManager.instance.EnableExtrude();
                extrudeToolButton.image.color = toggledButtonColor;
                extrudeToolToggled = true;
            }
        }

        public void HandleLockTool()
        {
            if (!lockToolToggled)
            {
                DisableAllTools();
                ToolManager.instance.EnableLock();
                extrudeToolButton.image.color = toggledButtonColor;
                lockToolToggled = true;
            }
        }

        public void HandleResetDefaults()
        {
            SetDefaultAbilities();
        }

        public void HandleAbilities()
        {
            HandleGrabVertices(grabVerticesToggle.isOn);
            HandleGrabEdges(grabEdgesToggle.isOn);
            HandleGrabFaces(grabFacesToggle.isOn);
            HandleAutoMergeVertices(autoMergeVerticesToggle.isOn);
            //HandleRaycastGrab(raycastGrabToggle.isOn);
        }

        #endregion

        #region Private Methods

        private void SetDefaultAbilities()
        {
            grabVerticesToggle.isOn = true;
            grabEdgesToggle.isOn = true;
            grabFacesToggle.isOn = true;
            autoMergeVerticesToggle.isOn = true;
            //raycastGrabToggle.isOn = false;

            HandleAbilities();
        }

        private void DisableAllTools()
        {
            DisableExtrudeTool();
            DisableLockTool();
        }

        private void DisableExtrudeTool()
        {
            if (extrudeToolToggled)
            {
                ToolManager.instance.DisableExtrude();
                extrudeToolButton.image.color = untoggledButtonColor;
                extrudeToolToggled = true;
            }
        }

        private void DisableLockTool()
        {
            if (lockToolToggled)
            {
                ToolManager.instance.DisableLock();
                lockToolButton.image.color = untoggledButtonColor;
                lockToolToggled = true;
            }
        }

        #endregion
    }
}

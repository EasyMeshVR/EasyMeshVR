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

        [SerializeField] private Color untoggledButtonColor;
        [SerializeField] private Color toggledButtonColor;

        [SerializeField] private Image extrudeToolImage;
        [SerializeField] private Image lockToolImage;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            extrudeToolToggled = false;
            lockToolToggled = false;

            extrudeToolImage.color = untoggledButtonColor;
            lockToolImage.color = untoggledButtonColor;

            DisableAllTools();
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

        public void HandleAutoMergeVertices(bool toggled)
        {
            if (toggled)
            {
                ToolManager.instance.EnableAutoMergeVertex();
            }
            else
            {
                ToolManager.instance.DisableAutoMergeVertex();
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
                extrudeToolImage.color = toggledButtonColor;
                extrudeToolToggled = true;
            }
            else
            {
                DisableExtrudeTool();
            }
        }

        public void HandleLockTool()
        {
            if (!lockToolToggled)
            {
                DisableAllTools();
                ToolManager.instance.EnableLock();
                lockToolImage.color = toggledButtonColor;
                lockToolToggled = true;
            }
            else
            {
                DisableLockTool();
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
            raycastGrabToggle.isOn = false;

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
                extrudeToolImage.color = untoggledButtonColor;
                extrudeToolToggled = false;
            }
        }

        private void DisableLockTool()
        {
            if (lockToolToggled)
            {
                ToolManager.instance.DisableLock();
                lockToolImage.color = untoggledButtonColor;
                lockToolToggled = false;
            }
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using EasyMeshVR.Multiplayer;

// All tools should be disabled by default, use this class to enable them

public class ToolManager : MonoBehaviour
{
    public static ToolManager instance { get; private set; }

    [SerializeField] public bool LockTool;
    [SerializeField] public bool extrudeTool;

    [SerializeField] public LockVertex lockScriptRay;
    [SerializeField] public LockVertex lockScriptGrab;

    [SerializeField] public Extrude extrudeScriptRay;
    [SerializeField] public Extrude extrudeScriptGrab;

    [SerializeField] public bool lockFaceRotation = false;
    [SerializeField] public bool lockEdgeRotation = false;

    /*List<XRGrabInteractable> vertexGrab = new List<XRGrabInteractable>();
    List<XRGrabInteractable> edgeGrab = new List<XRGrabInteractable>();
    List<XRGrabInteractable> faceGrab = new List<XRGrabInteractable>();*/

    public bool grabVertex = true;
    public bool grabEdge = true;
    public bool grabFace = true;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }

    public void EnableLock()
    {
        Debug.Log("Enabling lock tool");

        LockTool = true;
        lockScriptRay.Enable();
        lockScriptGrab.Enable();
    }

    public void DisableLock()
    {
        Debug.Log("Disabling lock tool");

        LockTool = false;
        lockScriptRay.Disable();
        lockScriptGrab.Disable();
    }

    public void EnableExtrude()
    {
        Debug.Log("Enabling extrude tool");

        extrudeTool = true;
        extrudeScriptGrab.Enable();
        extrudeScriptRay.Enable();
    }

    public void DisableExtrude()
    {
        Debug.Log("Disabling extrude tool");

        extrudeTool = false;
        extrudeScriptGrab.Disable();
        extrudeScriptRay.Disable();
    }

    public void EnableVertex()
    {
        Debug.Log("Enabling vertices");

        grabVertex = true;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Vertex v in meshRebuilder.vertexObjects)
            {
                v.gameObject.SetActive(true);
            }
        }
    }

    public void DisableVertex()
    {
        Debug.Log("Disabling vertices");

        grabVertex = false;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Vertex v in meshRebuilder.vertexObjects)
            {
                v.gameObject.SetActive(false);
            }
        }
    }

    public void EnableEdge()
    {
        Debug.Log("Enabling edges");

        grabEdge = true;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Edge e in meshRebuilder.edgeObjects)
            {
                e.gameObject.SetActive(true);
            }
        }
    }

    public void DisableEdge()
    {
        Debug.Log("Disabling edges");

        grabEdge = false;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Edge e in meshRebuilder.edgeObjects)
            {
                e.gameObject.SetActive(false);
            }
        }
    }

    public void EnableFace()
    {
        Debug.Log("Enabling faces");

        grabFace = true;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Face f in meshRebuilder.faceObjects)
            {
                f.gameObject.SetActive(true);
            }
        }
    }

    public void DisableFace()
    {
        Debug.Log("Disabling faces");

        grabFace = false;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Face f in meshRebuilder.faceObjects)
            {
                f.gameObject.SetActive(false);
            }
        }
    }

    public void ToggleLockFaceRotation()
    {
        lockFaceRotation = !lockFaceRotation;
    }

    public void ToggleLockEdgeRotation()
    {
        lockEdgeRotation = !lockEdgeRotation;
    }
}

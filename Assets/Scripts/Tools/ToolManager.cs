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

    public bool grabVertex = true;
    public bool grabEdge = true;
    public bool grabFace = true;
    public bool autoMergeVertex = false;

    private void Awake()
    {
        instance = this;
    }

    public void EnableLock()
    {
        LockTool = true;
        lockScriptRay.Enable();
        lockScriptGrab.Enable();
    }

    public void DisableLock()
    {
        LockTool = false;
        lockScriptRay.Disable();
        lockScriptGrab.Disable();
    }

    public void EnableExtrude()
    {
        extrudeTool = true;
        extrudeScriptGrab.Enable();
        extrudeScriptRay.Enable();
    }

    public void DisableExtrude()
    {
        extrudeTool = false;
        extrudeScriptGrab.Disable();
        extrudeScriptRay.Disable();
    }

    public void EnableVertex()
    {
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
        grabFace = false;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Face f in meshRebuilder.faceObjects)
            {
                f.gameObject.SetActive(false);
            }
        }
    }

    public void EnableAutoMergeVertex()
    {
        autoMergeVertex = true;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Vertex v in meshRebuilder.vertexObjects)
            {
                v.GetComponent<Merge>().enabled = true;
            }
        }
    }

    public void DisableAutoMergeVertex()
    {
        autoMergeVertex = false;

        foreach (MeshRebuilder meshRebuilder in NetworkMeshManager.instance.meshRebuilders)
        {
            foreach (Vertex v in meshRebuilder.vertexObjects)
            {
                v.GetComponent<Merge>().enabled = false;
            }
        }
    }
}

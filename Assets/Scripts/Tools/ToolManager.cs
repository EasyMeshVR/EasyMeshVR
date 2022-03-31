using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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


    List<XRGrabInteractable> vertexGrab = new List<XRGrabInteractable>();
    List<XRGrabInteractable> edgeGrab = new List<XRGrabInteractable>();


    public bool grabVertex = true;
    public bool grabEdge = false;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        checkImport();

        DisableLock();
    }

    void checkImport()
    {
        vertexGrab.Clear();
        edgeGrab.Clear();
        GameObject [] vertices = GameObject.FindGameObjectsWithTag("Vertex");
        GameObject [] edges = GameObject.FindGameObjectsWithTag("Edge");

        foreach(GameObject vertex in vertices)
        {
            vertexGrab.Add(vertex.GetComponent<XRGrabInteractable>());
        }
        foreach(GameObject e in edges)
            edgeGrab.Add(e.GetComponent<XRGrabInteractable>());
    }

    // For now use update to check but when this gets hooked up to the UI another script will call the functions
    void Update()
    {
        if(LockTool)
            EnableLock();
            
        if(!LockTool)
            DisableLock();

        if(extrudeTool)
            EnableExtrude();
            
        if(!extrudeTool)
            DisableExtrude();

        if(grabVertex)
            EnableVertex();

        if(grabEdge)
            EnableEdge();

        if(!grabEdge)
            DisableEdge();

        if(!grabVertex)
            DisableVertex();
    }

    void EnableLock()
    {
       lockScriptRay.Enable();
       lockScriptGrab.Enable();
    }

    void DisableLock()
    {
       lockScriptRay.Disable();
       lockScriptGrab.Disable();
    }

     void EnableExtrude()
    {
       extrudeScriptGrab.Enable();
       extrudeScriptRay.Enable();
    }

    void DisableExtrude()
    {
       extrudeScriptGrab.Disable();
       extrudeScriptRay.Disable();
    }

    void EnableVertex()
    {
        Debug.Log("in enableVertex");
        checkImport();
        foreach(XRGrabInteractable v in vertexGrab)
            v.enabled = true;
    }

    void DisableVertex()
    {
        checkImport();
        foreach(XRGrabInteractable v in vertexGrab)
            v.enabled = false;
    }

    void DisableEdge()
    {   
        checkImport();
        foreach(XRGrabInteractable e in edgeGrab)
            e.enabled = false;
    }

    void EnableEdge()
    {
        Debug.Log("in enableEdge");
        checkImport();
        foreach(XRGrabInteractable e in edgeGrab)
            e.enabled = true;
    }
}

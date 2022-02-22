using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// All tools should be disabled by default, use this class to enable them

public class ToolManager : MonoBehaviour
{
    [SerializeField] public bool LockTool;
    List<LockVertex> lockScripts = new List<LockVertex>();

    List<XRGrabInteractable> vertexGrab = new List<XRGrabInteractable>();
    List<XRGrabInteractable> edgeGrab = new List<XRGrabInteractable>();


    public bool grabVertex = true;
    public bool grabEdge = false;


     void Start()
    {
        checkImport();

        DisableLock();
    }

    void checkImport()
    {
        lockScripts.Clear();
        vertexGrab.Clear();
        edgeGrab.Clear();
        GameObject [] vertices = GameObject.FindGameObjectsWithTag("Vertex");
        GameObject [] edges = GameObject.FindGameObjectsWithTag("Edge");

        foreach(GameObject vertex in vertices)
        {
            lockScripts.Add(vertex.GetComponent<LockVertex>());
            vertexGrab.Add(vertex.GetComponent<XRGrabInteractable>());
        }
        foreach(GameObject e in edges)
            edgeGrab.Add(e.GetComponent<XRGrabInteractable>());
    }

    // For now use update to check but when this gets hooked up to the UI another script will call the functions
    void Update()
    {
        // Commented these out for now, it was throwing continuous errors on model import
        if(LockTool)
            EnableLock();
            
        if(!LockTool)
            DisableLock();

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
        foreach(LockVertex script in lockScripts)
           // script.enabled = true;
           script.isEnabled = true;
    }

    void DisableLock()
    {
        foreach(LockVertex script in lockScripts)
            //script.enabled = false;
            script.isEnabled = false;
    }

    void EnableVertex()
    {
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
        checkImport();
        foreach(XRGrabInteractable e in edgeGrab)
            e.enabled = true;
    }
}

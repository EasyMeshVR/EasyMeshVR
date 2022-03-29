using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// All tools should be disabled by default, use this class to enable them

public class ToolManager : MonoBehaviour
{
    [SerializeField] public bool LockTool;
    [SerializeField] public bool extrudeTool;

    [SerializeField] LockVertex lockScriptRay;
    [SerializeField] LockVertex lockScriptGrab;

    [SerializeField] Extrude extrudeScriptRay;
    [SerializeField] Extrude extrudeScriptGrab;



    [SerializeField] public bool MergeTool;
    List<Merge> mergeScripts = new List<Merge>();


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
        vertexGrab.Clear();
        edgeGrab.Clear();
        GameObject [] vertices = GameObject.FindGameObjectsWithTag("Vertex");
        GameObject [] edges = GameObject.FindGameObjectsWithTag("Edge");

        foreach(GameObject vertex in vertices)
        {
            vertexGrab.Add(vertex.GetComponent<XRGrabInteractable>());
        }

       // print("num lock scripts " + lockScripts.Count);
        //DisableLock();


        //foreach (GameObject vertex in vertices)

       // print("num merge scripts " + mergeScripts.Count);
        //DisableMerge();
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

        if (MergeTool)
            EnableMerge();

        if (!MergeTool)
            DisableMerge();

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

    void EnableMerge()
    {
        foreach (Merge script in mergeScripts)
            script.isEnabled = true;
    }

    void DisableMerge()
    {
        foreach (Merge script in mergeScripts)
            script.isEnabled = false;
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

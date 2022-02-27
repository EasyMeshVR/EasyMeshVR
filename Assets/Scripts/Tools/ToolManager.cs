using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All tools should be disabled by default, use this class to enable them

public class ToolManager : MonoBehaviour
{
    [SerializeField] public bool LockTool;
    List<LockVertex> lockScripts = new List<LockVertex>();

    [SerializeField] public bool MergeTool;
    List<Merge> mergeScripts = new List<Merge>();

    void Start()
    {
        GameObject [] vertices = GameObject.FindGameObjectsWithTag("Vertex");

        foreach(GameObject vertex in vertices)
            lockScripts.Add(vertex.GetComponent<LockVertex>());

        print("num lock scripts " + lockScripts.Count);
        DisableLock();

        // ---------------------------------------------------------------

        foreach (GameObject vertex in vertices)
            mergeScripts.Add(vertex.GetComponent<Merge>());

        print("num merge scripts " + mergeScripts.Count);
        DisableMerge();
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
}

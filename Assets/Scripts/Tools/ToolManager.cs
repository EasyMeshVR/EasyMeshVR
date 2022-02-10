using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// All tools should be disabled by default, use this class to enable them

public class ToolManager : MonoBehaviour
{
    [SerializeField] public bool LockTool;
    List<LockVertex> lockScripts = new List<LockVertex>();

     void Start()
    {
        GameObject [] vertices = GameObject.FindGameObjectsWithTag("Vertex");

        foreach(GameObject vertex in vertices)
            lockScripts.Add(vertex.GetComponent<LockVertex>());

        print("num scripts " + lockScripts.Count);
        DisableLock();
    }

    // For now use update to check but when this gets hooked up to the UI another script will call the functions
    void Update()
    {
        if(LockTool)
            EnableLock();
            
        if(!LockTool)
            DisableLock();
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;


public class LockVertex : ToolClass
{
    [SerializeField] Material locked;     // red
    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    public XRGrabInteractable vertexGrabInteractable;

    public PulleyLocomotion pulleyLocomotion;
    public GameObject editingSpace;
    public GameObject currentVertex;
    public GameObject currentEdge;

    //new public bool isEnabled = false;
    public bool inRadius = false;

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;

    int selectedVertex;
    int selectedEdge;
    

    MeshRenderer materialSwap;
    private bool hover = false;
    private float holdTime = 0f;

    bool holdFinish = false;

   void OnEnable()
    {
        editingSpace = GameObject.Find("EditingSpace");
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        leftSphere = GameObject.Find("LeftRadius").GetComponent<SphereCollider>();
        rightSphere = GameObject.Find("RightRadius").GetComponent<SphereCollider>();
    }

    public override void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
        holdTime = 0f;
    }

    public override void PrimaryAction()
    {
        if(!inRadius)
            return;

        if(currentVertex == null)
            return;
        
        if(currentVertex.GetComponent<MoveVertices>().isLocked)
            return;
        Lock();
    }

    public override void SecondaryAction()
    {
        if(!inRadius)
            return;

        if(currentVertex == null)
            return;
        
        if(!currentVertex.GetComponent<MoveVertices>().isLocked)
            return;

        Unlock();
    }

    void Lock()
    {
        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        vertexGrabInteractable.enabled = false;
        materialSwap.material = locked;

        currentVertex.GetComponent<MoveVertices>().isLocked = true;
        return;
    }

    void Unlock()
    {
        materialSwap = currentVertex.GetComponent<MeshRenderer>();
        vertexGrabInteractable.enabled = true;
        materialSwap.material = unselected;
        currentVertex.GetComponent<MoveVertices>().isLocked = false;
        return;
    }

    // Unlock all vertices if unlock button is held
    // Needs a visual attatched to indicate
    // // There's supposed to be a way to do this with the new input system but I can't find out how
    //  getting rid of hold to unlock for now since script is no longer attatched to vertices

    // void Update()
    // {
    //     if(secondaryButtonPressed)
    //         holdTime += Time.deltaTime;

    //      if(holdTime >= 1.5f)
    //         Unlock();
    // }

    // check sphere collision of controllers
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vertex"))
        {
            // get vertex infoo
            currentVertex = other.gameObject; 
            selectedVertex = currentVertex.GetComponent<Vertex>().id;
            vertexGrabInteractable = currentVertex.GetComponent<XRGrabInteractable>();
            print("test vertex id " + selectedVertex);
            inRadius = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        inRadius = false;
        currentVertex = null;
    }
}
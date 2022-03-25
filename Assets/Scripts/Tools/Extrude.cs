using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;
using EasyMeshVR.Core;

public class Extrude : ToolClass
{
    // [SerializeField] Material locked;     // red
    // [SerializeField] Material unselected;   // gray
    // [SerializeField] Material hovered;      // orange

    [SerializeField] SwitchControllers switchControllers;

    [SerializeField] ToolRaycast ray;


    public XRGrabInteractable vertexGrabInteractable;

   // public PulleyLocomotion pulleyLocomotion;
    //public GameObject editingSpace;
    public GameObject currentFace;
   // public GameObject currentEdge;

    public bool inRadius = false;

    public SphereCollider leftSphere;
    public SphereCollider rightSphere;

    public int selectedFace;

    Vertex vertex1;
    Vertex vertex2;
    Vertex vertex3;

    Mesh mesh;

    GameObject model;

    Face thisFace;

    public MeshRebuilder meshRebuilder;



    

   // MeshRenderer materialSwap;

    private bool hover = false;
    //private float holdTime = 0f;

   // bool holdFinish = false;

   void OnEnable()
    {
       // editingSpace = GameObject.Find("EditingSpace");
       // pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();


        leftSphere = GameObject.Find("LeftRadius").GetComponent<SphereCollider>();
        rightSphere = GameObject.Find("RightRadius").GetComponent<SphereCollider>();
    }

    public override void secondaryButtonEnd(InputAction.CallbackContext context)
    {
        secondaryButtonPressed = false;
       // holdTime = 0f;
    }

    public override void PrimaryAction()
    {
        if(!inRadius)
            return;

        if(currentFace == null)
            return;
        
       // if(currentFace.GetComponent<MoveVertices>().isLocked)
            //return;
        extrudeFace();
    }

    // public override void SecondaryAction()
    // {
    //     if(!inRadius)
    //         return;

    //     if(currentFace == null)
    //         return;
        
    //     Unlock();
    // }

    // Change material, disable vertex grab interactable, set boolean
    void extrudeFace()
    {
        // calculating normals is done in mesh rebuilder

        // align new vertices w/ face nooraml
        Vector3 new1 = vertex1.transform.localPosition + ((thisFace.normal  + vertex1.transform.localPosition *.005f).normalized );
        Vector3 new2 = vertex2.transform.localPosition + ((thisFace.normal  + vertex2.transform.localPosition *.005f).normalized );
        Vector3 new3 = vertex3.transform.localPosition + ((thisFace.normal  + vertex3.transform.localPosition *.005f).normalized );

        // Add new vertices to list -> to array
        List<Vector3> vertList = new List<Vector3>(MeshRebuilder.instance.vertices);
        vertList.Add(new1);
        vertList.Add(new2);
        vertList.Add(new3);
        Vector3[] vertices = vertList.ToArray();

        int newVert1 = vertices.Length - 3;
        int newVert2 = vertices.Length - 2;
        int newVert3 = vertices.Length - 1;

        // new face indices = vertices.Length - 3, etc 
        // old face indices = face.vert1 etc

        // Add new triangles to list -> to array
        List<int> triList = new List<int>(MeshRebuilder.instance.triangles);

        // new1, new3, new2
        triList.Add(newVert1);
        triList.Add(newVert3);
        triList.Add(newVert2);

        // new1, old1, new2
        triList.Add(newVert1);
        triList.Add(thisFace.vert1);
        triList.Add(newVert2);

        // new2, old1, old2
        triList.Add(newVert2);
        triList.Add(thisFace.vert1);
        triList.Add(thisFace.vert2);
        
        //new3, new2, old2
        triList.Add(newVert3);
        triList.Add(newVert2);
        triList.Add(thisFace.vert2);

        //new3, old2, old3
        triList.Add(newVert3);
        triList.Add(thisFace.vert2);
        triList.Add(thisFace.vert3);

        //new3, old3, old1
        triList.Add(newVert3);
        triList.Add(thisFace.vert3);
        triList.Add(thisFace.vert1);

        //new3, old1, new1
        triList.Add(newVert3);
        triList.Add(thisFace.vert1);
        triList.Add(newVert1);

        // new123
        triList.Add(newVert1);
        triList.Add(newVert2);
        triList.Add(newVert3);
        int[] tris = triList.ToArray();

        // Update mesh data
        mesh.vertices = vertices;
        meshRebuilder.vertices = vertices;
        mesh.triangles = tris;
        meshRebuilder.triangles = tris;
        mesh.RecalculateNormals();

        // Update mesh visuals
        //  meshRebuilder.RemoveDuplicates();
        meshRebuilder.removeVisuals();
        meshRebuilder.CreateVisuals();
        return;
    }

    // Get face info and 3 vertices
    public void OnTriggerEnter(Collider other)
    {
        checkImport();
        if (other.CompareTag("Face"))
        {
            currentFace = other.gameObject; 
            thisFace = currentFace.GetComponent<Face>();
            selectedFace = thisFace.id;

            vertex1 = meshRebuilder.vertexObjects[thisFace.vert1];
            vertex2 = meshRebuilder.vertexObjects[thisFace.vert2];
            vertex3 = meshRebuilder.vertexObjects[thisFace.vert3];
            inRadius = true;
        }
    }

    // Clear data on exiting radius
    public void OnTriggerExit(Collider other)
    {
        if(!switchControllers.rayActive)
        {
            inRadius = false;
            currentFace = null;
            thisFace = null;
            vertex1 = null;
            vertex2 = null;
            vertex3 = null;
        }
    }

    void checkImport()
    {
        //editingSpace = GameObject.Find("EditingSpace");
        meshRebuilder = GameObject.FindObjectOfType<MeshRebuilder>();
        model = meshRebuilder.model;
        mesh = model.GetComponent<MeshFilter>().mesh;
    }

    // Raycast checking
    void Update()
    {
        if(switchControllers.rayActive)
        {
            if(ray.hitFace)
            {
                checkImport();
                currentFace = ray.hit.transform.gameObject;
                thisFace = currentFace.GetComponent<Face>();
                selectedFace = thisFace.id;
                vertex1 = meshRebuilder.vertexObjects[thisFace.vert1];
                vertex2 = meshRebuilder.vertexObjects[thisFace.vert2];
                vertex3 = meshRebuilder.vertexObjects[thisFace.vert3];
                inRadius = true;
                if(primaryButtonPressed)
                    extrudeFace();
            }
            else
            {
                inRadius = false;
                currentFace = null;
                thisFace = null;
                vertex1 = null;
                vertex2 = null;
                vertex3 = null;
            }
        }
    }
}

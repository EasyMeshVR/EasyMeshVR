using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveVertices : MonoBehaviour
{
    [SerializeField] GameObject model;

    [SerializeField] XRGrabNetworkInteractable grabInteractable;

    [SerializeField] Material unselected;
    [SerializeField] Material hovered;
    [SerializeField] Material selected;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;
    Vector3[] vertices;
    int[] triangles; // we shouldn't need triangles here, but just in case

    Vector3 originalPosition = new Vector3();
    int selectedVertex = new int();

    void Start()
    {
        
    }

    void OnEnable()
    {
        // Get the editing model's MeshFilter
        mesh = this.gameObject.GetComponentInParent<MeshFilter>().mesh;

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Copy the vertices
        vertices = mesh.vertices;
        triangles = mesh.triangles;



        // 
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);

        // This needs to be a whileSelected kind of thing, rather than just once when it's pressed
        grabInteractable.selectEntered.AddListener(GrabPulled);
    }

    void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(HoverOver);

        grabInteractable.hoverExited.RemoveListener(HoverExit);

        grabInteractable.selectEntered.RemoveListener(GrabPulled);
    }

    // Get original position of Vertex before moving
    // Set material to Selected (change name to hover)
    void HoverOver(HoverEnterEventArgs arg0)
    {
        materialSwap.material = selected;

        originalPosition = gameObject.transform.position;

        // Use its original position to find the reference in the vertices array so we can access it quicker later
        // i.e. we get its index instead of having to compare its Vector3 over and over again
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] == originalPosition)
            {
                selectedVertex = i;
            }
        }
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        materialSwap.material = unselected;
    }

    // Pull vertex to hand and update position on GameObject and in Mesh and change material
    void GrabPulled(SelectEnterEventArgs arg0)
    {
        vertices[selectedVertex] = gameObject.transform.position;

        UpdateMesh();
    }

    /*
    void Update()
    {
        Vector3 originalPosition = new Vector3();
        int quickRef = new int();

        // Check controller raycast and store the position of the Vertex we're hovering over
        // This is explicitly for hovering, and we only want to store its position BEFORE it moves
        // if ( check raycast )
        {
            originalPosition = gameObject.transform.position;

            // Use its original position to find the reference in the vertices array so we can access it quicker later
            // i.e. we get its index instead of having to compare its Vector3 over and over again
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i] == originalPosition)
                {
                    quickRef = i;
                }
            }
        }

        // If the grab button is pressed and the Vertex is moving, constantly save its updated position
        // As we save it, we update that position for the appropriate vertex in the vertices array
        // if ( grab button is pressed and vertex is selected --and if raycast isn't being used-- )
        {
            vertices[quickRef] = gameObject.transform.position;
        }

        // Use the updated vertices array and update the mesh as we move the Vertex across the screen
        UpdateMesh();
    }
    */

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}

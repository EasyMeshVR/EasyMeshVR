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

    Vector3 originalPosition = new Vector3();
    int selectedVertex = new int();

    bool grabHeld = false;

    void OnEnable()
    {
        // Get the editing model's MeshFilter
        model = GameObject.FindGameObjectWithTag("Model");
        mesh = model.GetComponent<MeshFilter>().mesh;

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Copy the vertices
        vertices = mesh.vertices;

        // Hover listeners to change vertex color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);

        // This needs to be a whileSelected kind of thing, rather than just once when it's pressed
        grabInteractable.selectEntered.AddListener(GrabPulled);
        grabInteractable.selectExited.AddListener(GrabReleased);
    }

    void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(HoverOver);

        grabInteractable.hoverExited.RemoveListener(HoverExit);

        grabInteractable.selectEntered.RemoveListener(GrabPulled);

        grabInteractable.selectExited.RemoveListener(GrabReleased);
    }

    // Get original position of Vertex before moving
    // Set material to Selected (change name to hover)
    void HoverOver(HoverEnterEventArgs arg0)
    {
        materialSwap.material = hovered;

        // Finds the correspoinding vertex on the mesh based off the GameObject's position
        // Using localPosition since it's a parent of the model
        originalPosition = transform.localPosition;

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
        materialSwap.material = selected;

        grabHeld = true;
    }

    // Stop updating the mesh data
    void GrabReleased(SelectExitEventArgs arg0)
    {
        materialSwap.material = unselected;

        grabHeld = false;
    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (grabHeld)
        {
            // Update the mesh filter's vertices to the vertex GameObject's position
            // This doesn't work too well, it sets the mesh's vertex to way higher than where the GameObject is
            // I think this has something to do with local vs world space, but transform.localPosition doesn't work either
            vertices[selectedVertex] = transform.position;

            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}

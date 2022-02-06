using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using EasyMeshVR.Core;

public class MoveVertices : MonoBehaviour
{
    [SerializeField] GameObject model;

    [SerializeField] XRGrabNetworkInteractable grabInteractable;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    GameObject editingSpace;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;
    Vector3[] vertices;
    Vertex thisvertex;

    // Vertex lookup
    Vector3 originalPosition;
    int selectedVertex;

    bool grabHeld = false;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        editingSpace = MeshRebuilder.instance.editingSpace;
        model = GameObject.FindGameObjectWithTag("Model");
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisvertex = GetComponent<Vertex>();

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Copy the vertices
        vertices = mesh.vertices;

        // Hover listeners to change vertex color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);

        // This checks if the grab has been pressed or released
        // Needs to be updated to trigger/button pinch controls
        grabInteractable.selectEntered.AddListener(GrabPulled);
        grabInteractable.selectExited.AddListener(GrabReleased);
    }

    // We don't need the control listeners if OnDisable() is ever called
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

        // Keep mesh filter updated with most recent mesh data changes
        vertices = mesh.vertices;

        // The selected vertex is just the saved id of this vertex representing its index
        // in the vertices array
        selectedVertex = thisvertex.id;

        // Old way of finding what vertex we just hovered over
        // Finds the correspoinding vertex on the mesh based off the GameObject's position
        // Using localPosition since it's a parent of the model
        //originalPosition = transform.localPosition;

        // Use its original position to find the reference in the vertices array so we can access it quicker later
        // i.e. we get its index instead of having to compare its Vector3 over and over again
        /*for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i] == originalPosition)
            {
                selectedVertex = i;
            }
        }*/
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        materialSwap.material = unselected;
    }

    // Pull vertex to hand and update position on GameObject and in Mesh and change material
    void GrabPulled(SelectEnterEventArgs arg0)
    {
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
            materialSwap.material = selected;

            // Update the mesh filter's vertices to the vertex GameObject's position

            // Calculate inverse scale vector
            Vector3 editingSpaceScale = editingSpace.transform.localScale;
            Vector3 inverseScale = new Vector3(
                1.0f / editingSpaceScale.x, 
                1.0f / editingSpaceScale.y, 
                1.0f / editingSpaceScale.z
            );

            // Translate, Scale, and Rotate the vertex position based on the current transform
            // of the editingSpace object.
            vertices[selectedVertex] =
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, transform.position - editingSpace.transform.position);

            UpdateMesh();
        }
    }

    // Update MeshFilter and re-draw in-game visuals
    void UpdateMesh()
    {
        // Update actual mesh data
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Look through visuals Dictionary to update mesh visuals (reconnect edges to vertices)
        foreach (var kvp in MeshRebuilder.visuals)
        {
            // Dictionary created in MeshRebuilder.cs
            // Dictionary<GameObject, List<int>>
            // GameObject = edge, List<int> = vertex 1 (origin), vertex 2

            // If either of the vertex values are the same as selectedVertex, it will update the edges that vertex is connected to
            if (kvp.Value[0] == selectedVertex || kvp.Value[1] == selectedVertex)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[kvp.Value[0]], vertices[kvp.Value[1]]);
                kvp.Key.transform.localPosition = (vertices[kvp.Value[0]] + vertices[kvp.Value[1]]) / 2;
                kvp.Key.transform.localScale = new Vector3(kvp.Key.transform.localScale.x, edgeDistance, kvp.Key.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                kvp.Key.transform.LookAt(transform, Vector3.up);
                kvp.Key.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }
    }
}
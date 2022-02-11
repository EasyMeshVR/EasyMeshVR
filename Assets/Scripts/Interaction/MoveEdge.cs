using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using EasyMeshVR.Core;

public class MoveEdge : MonoBehaviour
{
    [SerializeField] GameObject model;

    [SerializeField] XRGrabInteractable grabInteractable;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;
    Vector3[] vertices;

    // Edge lookup
    Vector3 originalPosition;
    GameObject selectedEdge;
    int selectedVertex1;
    int selectedVertex2;
    GameObject vertex1;
    GameObject vertex2;

    bool grabHeld = false;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        model = GameObject.FindGameObjectWithTag("Model");
        mesh = model.GetComponent<MeshFilter>().mesh;

        // Editing space objects
        editingSpace = MeshRebuilder.instance.editingSpace;
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Copy the vertices
        vertices = mesh.vertices;

        // Hover listeners to change edge color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);

        // This checks if the grab has been pressed or released
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
        if (pulleyLocomotion.isMovingEditingSpace)
            return;

        materialSwap.material = hovered;

        // Keep mesh filter updated with most recent mesh data changes
        vertices = mesh.vertices;

        // Finds the correspoinding edge on the mesh based off the GameObject's position
        // Using localPosition since it's a parent of the model
        originalPosition = transform.localPosition;

        foreach (var kvp in MeshRebuilder.visuals)
        {
            if ((kvp.Key).transform.position == originalPosition)
            {
                selectedEdge = kvp.Key;
                selectedVertex1 = kvp.Value[0];
                selectedVertex2 = kvp.Value[1];
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
        if (pulleyLocomotion.isMovingEditingSpace)
            return;

        // Find the two vertices that are connected to the edge we grabbed
        vertex1 = GameObject.Find("Vertex" + selectedVertex1.ToString());
        vertex2 = GameObject.Find("Vertex" + selectedVertex2.ToString());

        // Parent the two vertices to the edge
        vertex1.transform.parent = selectedEdge.transform;
        vertex2.transform.parent = selectedEdge.transform;

        grabHeld = true;
        pulleyLocomotion.isMovingVertex = true;
    }

    // Stop updating the mesh data
    void GrabReleased(SelectExitEventArgs arg0)
    {
        materialSwap.material = unselected;

        // Unparent the vertices from the edge
        vertex1.transform.parent = model.transform;
        vertex2.transform.parent = model.transform;

        grabHeld = false;
        pulleyLocomotion.isMovingVertex = false;
    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (pulleyLocomotion.isMovingEditingSpace)
        {
            grabInteractable.enabled = false;
            return;
        }
        grabInteractable.enabled = true;

        if (grabHeld)
        {
            materialSwap.material = selected;

            // Update the mesh filter's vertices to the vertices' GameObjects' positions
            // Subtracts model's offset if it's not directly on (0,0,0)
            vertex1.transform.parent = model.transform;
            vertex2.transform.parent = model.transform;

            // vertices[selectedVertex1] = vertex1.transform.localPosition - model.transform.position;
            // vertices[selectedVertex2] = vertex2.transform.localPosition - model.transform.position;

            // Calculate inverse scale vector
            Vector3 editingSpaceScale = editingSpace.transform.localScale;
            Vector3 inverseScale = new Vector3(
                1.0f / editingSpaceScale.x,
                1.0f / editingSpaceScale.y,
                1.0f / editingSpaceScale.z
            );

            // Translate, Scale, and Rotate the vertex position based on the current transform of the editingSpace object.
            vertices[selectedVertex1] =
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex1.transform.position - editingSpace.transform.position);

            vertices[selectedVertex2] =
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex2.transform.position - editingSpace.transform.position);

            UpdateMesh();

            vertex1.transform.parent = selectedEdge.transform;
            vertex2.transform.parent = selectedEdge.transform;
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

            // If either of the vertex values are the same as selectedVertex1, it will update the edges that vertex is connected to
            if (kvp.Value[0] == selectedVertex1 || kvp.Value[1] == selectedVertex1)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[kvp.Value[0]], vertices[kvp.Value[1]]);
                kvp.Key.transform.localPosition = (vertices[kvp.Value[0]] + vertices[kvp.Value[1]]) / 2;
                kvp.Key.transform.localScale = new Vector3(kvp.Key.transform.localScale.x, edgeDistance, kvp.Key.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                kvp.Key.transform.LookAt(vertex1.transform, Vector3.up);
                kvp.Key.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }

            // If either of the vertex values are the same as selectedVertex2, it will update the edges that vertex is connected to
            if (kvp.Value[0] == selectedVertex2 || kvp.Value[1] == selectedVertex2)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[kvp.Value[0]], vertices[kvp.Value[1]]);
                kvp.Key.transform.localPosition = (vertices[kvp.Value[0]] + vertices[kvp.Value[1]]) / 2;
                kvp.Key.transform.localScale = new Vector3(kvp.Key.transform.localScale.x, edgeDistance, kvp.Key.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                kvp.Key.transform.LookAt(vertex2.transform, Vector3.up);
                kvp.Key.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using EasyMeshVR.Multiplayer;

public class MoveVertices : MonoBehaviour
{
    [SerializeField] GameObject model;

    [SerializeField] XRGrabInteractable grabInteractable;
    [SerializeField] LockVertex lockVertex;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;

    // Vertex lookup
    Vector3 originalPosition;
    Vertex thisvertex;
    int selectedVertex;

    bool grabHeld = false;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        model = GameObject.FindGameObjectWithTag("Model");
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisvertex = GetComponent<Vertex>();

        // Editing space objects
        editingSpace = MeshRebuilder.instance.editingSpace;
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Hover listeners to change vertex color
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
        MeshRebuilder.instance.vertices = mesh.vertices;

        // The selected vertex is just the saved id of this vertex representing its index in the vertices array
        selectedVertex = thisvertex.id;
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

        grabHeld = true;
        pulleyLocomotion.isMovingVertex = true;
    }

    // Stop updating the mesh data
    void GrabReleased(SelectExitEventArgs arg0)
    {
        materialSwap.material = unselected;

        grabHeld = false;

        // Synchronize the position of the mesh vertex by sending a cached event to other players
        NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex], selectedVertex, true, true);
        pulleyLocomotion.isMovingVertex = false;
    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (pulleyLocomotion.isMovingEditingSpace || lockVertex.isLocked || thisvertex.isHeldByOther)
        {
            grabInteractable.enabled = false;
            return;
        }
        grabInteractable.enabled = true;

        if (grabHeld)
        {
            materialSwap.material = selected;

            // Update the mesh filter's vertices to the vertex GameObject's position
            UpdateVertex(transform, selectedVertex);
            UpdateMesh(selectedVertex);

            // Continuously synchronize the position of the vertex without caching it until we release it
            NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex], selectedVertex, false, false);
        }
    }

    public void UpdateVertex(Transform transform, int index)
    {
        Vector3 editingSpaceScale = editingSpace.transform.localScale;

        // Handle divide by zero error
        if (editingSpaceScale.x == 0 || editingSpaceScale.y == 0 || editingSpaceScale.z == 0)
        {
            return;
        }

        // Calculate inverse scale vector based on editing space scale
        Vector3 inverseScale = new Vector3(
            1.0f / editingSpaceScale.x,
            1.0f / editingSpaceScale.y,
            1.0f / editingSpaceScale.z
        );

        // Translate, Scale, and Rotate the vertex position based on the current transform
        // of the editingSpace object.
        MeshRebuilder.instance.vertices[index] =
            Quaternion.Inverse(editingSpace.transform.rotation)
            * Vector3.Scale(inverseScale, transform.position - editingSpace.transform.position);
    }

    // Update MeshFilter and re-draw in-game visuals
    public void UpdateMesh(int index)
    {
        Vector3[] vertices = MeshRebuilder.instance.vertices;

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
            if (kvp.Value[0] == index || kvp.Value[1] == index)
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
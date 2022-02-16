using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using EasyMeshVR.Multiplayer;

public class MoveEdge : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    GameObject model;

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;

    // Edge lookup
    Edge thisedge;
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
        model = MeshRebuilder.instance.model;
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisedge = GetComponent<Edge>();

        // Editing space objects
        editingSpace = MeshRebuilder.instance.editingSpace;
        pulleyLocomotion = editingSpace.GetComponent<PulleyLocomotion>();

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

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
        MeshRebuilder.instance.vertices = mesh.vertices;

        selectedEdge = thisedge.gameObject;
        selectedVertex1 = thisedge.vert1;
        selectedVertex2 = thisedge.vert2;   
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

        vertex1 = MeshRebuilder.instance.vertexObjects[selectedVertex1].gameObject;
        vertex2 = MeshRebuilder.instance.vertexObjects[selectedVertex2].gameObject;

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

        // Synchronize the position of the mesh vertex by sending a cached event to other players
        NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex1], selectedVertex1, true, true);
        NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex2], selectedVertex2, true, true);
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
            //vertex1.transform.parent = model.transform;
            //vertex2.transform.parent = model.transform;

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

            // Translate, Scale, and Rotate the vertex position based on the current transform of the editingSpace object.
            MeshRebuilder.instance.vertices[selectedVertex1] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex1.transform.position - editingSpace.transform.position);

            MeshRebuilder.instance.vertices[selectedVertex2] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex2.transform.position - editingSpace.transform.position);

            UpdateMesh();

            //vertex1.transform.parent = selectedEdge.transform;
            //vertex2.transform.parent = selectedEdge.transform;

            // Continuously synchronize the position of the vertex without caching it until we release it
            NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex1], selectedVertex1, false, false);
            NetworkMeshManager.instance.SynchronizeMeshVertexPull(MeshRebuilder.instance.vertices[selectedVertex2], selectedVertex2, false, false);
        }
    }

    // Update MeshFilter and re-draw in-game visuals
    void UpdateMesh()
    {
        Vector3[] vertices = MeshRebuilder.instance.vertices;

        // Update actual mesh data
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Look through visuals Dictionary to update mesh visuals (reconnect edges to vertices)
        foreach (Edge edge in MeshRebuilder.instance.edgeObjects)
        {
            if (edge.id == thisedge.id) continue;

            GameObject edgeObject = edge.gameObject;
            int vert1 = edge.vert1;
            int vert2 = edge.vert2;

            // If either of the vertex values are the same as selectedVertex1, it will update the edges that vertex is connected to
            if (vert1 == selectedVertex1 || vert2 == selectedVertex1)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(vertex1.transform, Vector3.up);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }

            // If either of the vertex values are the same as selectedVertex2, it will update the edges that vertex is connected to
            if (vert1 == selectedVertex2 || vert2 == selectedVertex2)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(vertex2.transform, Vector3.up);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }
    }
}
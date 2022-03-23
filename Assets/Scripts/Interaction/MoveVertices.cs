using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using EasyMeshVR.Multiplayer;

public class MoveVertices : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;
   // [SerializeField] LockVertex lockVertex;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    //[SerializeField] SwitchControllers switchControllers;


    GameObject model;

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    public bool isLocked;

    // Mesh data
    Mesh mesh;
    MeshRenderer materialSwap;

    // Vertex lookup
    Vertex thisvertex;
    int selectedVertex;

    bool grabHeld = false;

    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        model = MeshRebuilder.instance.model;
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisvertex = GetComponent<Vertex>();

        //switchControllers = GameObject.Find("ToolManager").GetComponent<SwitchControllers>();

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


        //if(switchControllers.rayActive)
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

        VertexPullEvent vertexEvent = new VertexPullEvent()
        {
            id = selectedVertex,
            vertexPos = MeshRebuilder.instance.vertices[selectedVertex],
            released = true,
            isCached = true,
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
        };

        // Synchronize the position of the mesh vertex by sending a cached event to other players
        NetworkMeshManager.instance.SynchronizeMeshVertexPull(vertexEvent);
        pulleyLocomotion.isMovingVertex = false;
    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (pulleyLocomotion.isMovingEditingSpace || isLocked || thisvertex.isHeldByOther)
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

            VertexPullEvent vertexEvent = new VertexPullEvent()
            {
                id = selectedVertex,
                vertexPos = MeshRebuilder.instance.vertices[selectedVertex],
                released = false,
                isCached = false,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            // Continuously synchronize the position of the vertex without caching it until we release it
            NetworkMeshManager.instance.SynchronizeMeshVertexPull(vertexEvent);
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
        foreach (Edge edge in MeshRebuilder.instance.edgeObjects)
        {
            GameObject edgeObject = edge.gameObject;
            int vert1 = edge.vert1;
            int vert2 = edge.vert2;

            // If either of the vertex values are the same as selectedVertex, it will update the edges that vertex is connected to
            if (vert1 == index || vert2 == index)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(transform, Vector3.up);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }

        foreach (Face face in MeshRebuilder.instance.faceObjects)
        {
            GameObject faceObject = face.gameObject;
            int vert1 = face.vert1;
            int vert2 = face.vert2;
            int vert3 = face.vert3;

            // If either of the vertex values are the same as selectedVertex, it will update the edges that vertex is connected to
            if (vert1 == index || vert2 == index || vert3 == index)
            {
                // // Set the edge's position to between the two vertices and scale it appropriately
                // float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                // edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                // edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // // Orient the edge to look at the vertices (specifically the one we're currently holding)
                // edgeObject.transform.LookAt(transform, Vector3.up);
                // edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);

                float totalX = vertices[vert1].x + vertices[vert2].x + vertices[vert3].x;
                float totalY = vertices[vert1].y + vertices[vert2].y + vertices[vert3].y;
                float totalZ = vertices[vert1].z + vertices[vert2].z + vertices[vert3].z;

                faceObject.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using EasyMeshVR.Multiplayer;

public class MoveEdge : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue
    [SerializeField] Material locked;     // gray with reduced opacity

    // [SerializeField] SwitchControllers switchControllers;

    GameObject model;

    // Editing Space Objects
    GameObject editingSpace;
    PulleyLocomotion pulleyLocomotion;

    // Mesh data
    Mesh mesh;
    MeshRebuilder meshRebuilder;
    public MeshRenderer materialSwap;
    Vector3 oldEdgePosition, oldVert1Position, oldVert2Position;

    // Edge lookup
    Edge thisedge;
    GameObject selectedEdge;
    int selectedVertex1;
    int selectedVertex2;
    Vertex vertex1;
    Vertex vertex2;
    public bool grabHeld = false;

    public bool isLocked = false;


    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        meshRebuilder = transform.parent.GetComponent<MeshRebuilder>();
        model = meshRebuilder.model;
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisedge = GetComponent<Edge>();
        //switchControllers = GameObject.Find("ToolManager").GetComponent<SwitchControllers>();

        // Editing space objects
        editingSpace = meshRebuilder.editingSpace;
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
        if (pulleyLocomotion.isMovingEditingSpace || thisedge.locked)
            return;

        //if(switchControllers.rayActive)
        materialSwap.material = hovered;

        // Keep mesh filter updated with most recent mesh data changes
        meshRebuilder.vertices = mesh.vertices;
        oldEdgePosition = thisedge.transform.position;
        oldVert1Position = meshRebuilder.vertices[thisedge.vert1];
        oldVert2Position = meshRebuilder.vertices[thisedge.vert2];
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        if (thisedge.locked)
            return;

        materialSwap.material = unselected;
    }

    // Pull vertex to hand and update position on GameObject and in Mesh and change material
    void GrabPulled(SelectEnterEventArgs arg0)
    {
        if (pulleyLocomotion.isMovingEditingSpace || thisedge.locked)
            return;

        SetActiveEdges(thisedge, false);

        vertex1 = meshRebuilder.vertexObjects[thisedge.vert1];
        vertex2 = meshRebuilder.vertexObjects[thisedge.vert2];

        thisedge.transform.parent = model.transform;

        // Parent the two vertices to the edge
        vertex1.transform.parent = thisedge.transform;
        vertex2.transform.parent = thisedge.transform;

        vertex1.gameObject.SetActive(false);
        vertex2.gameObject.SetActive(false);

        grabHeld = true;
        pulleyLocomotion.isMovingVertex = true;
    }

    // Stop updating the mesh data
    void GrabReleased(SelectExitEventArgs arg0)
    {
        if (thisedge.locked)
            return;

        SetActiveEdges(thisedge, true);

        materialSwap.material = unselected;

        // Unparent the vertices from the edge
        vertex1.transform.parent = model.transform;
        vertex2.transform.parent = model.transform;

        if (ToolManager.instance.grabVertex)
        {
            vertex1.gameObject.SetActive(true);
            vertex2.gameObject.SetActive(true);
        }

        grabHeld = false;

        Vector3 vertex1Pos = meshRebuilder.vertices[thisedge.vert1];
        Vector3 vertex2Pos = meshRebuilder.vertices[thisedge.vert2];

        // Synchronize the position of the mesh vertex by sending a cached event to other players
        EdgePullEvent edgeEvent = new EdgePullEvent
        {
            id = thisedge.id,
            meshId = meshRebuilder.id,
            vert1 = thisedge.vert1,
            vert2 = thisedge.vert2,
            oldPosition = oldEdgePosition,
            position = thisedge.transform.position,
            oldVertex1Pos = oldVert1Position,
            vertex1Pos = vertex1Pos,
            oldVertex2Pos = oldVert2Position,
            vertex2Pos = vertex2Pos,
            isCached = true,
            released = true,
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
        };

        AddMoveEdgeOpStep(edgeEvent);

        NetworkMeshManager.instance.SynchronizeMeshEdgePull(edgeEvent);
        pulleyLocomotion.isMovingVertex = false;
    }

    public void AddMoveEdgeOpStep(EdgePullEvent edgeEvent)
    {
        Step step = new Step();
        MoveEdgeOp op = new MoveEdgeOp(edgeEvent.meshId, edgeEvent.id, edgeEvent.oldPosition, edgeEvent.position,
                                       edgeEvent.oldVertex1Pos, edgeEvent.vertex1Pos, edgeEvent.oldVertex2Pos, edgeEvent.vertex2Pos);
        step.AddOp(op);
        StepExecutor.instance.AddStep(step);
    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (pulleyLocomotion.isMovingEditingSpace || thisedge.isHeldByOther || thisedge.locked)
        {
            return;
        }

        if (grabHeld)
        {
            materialSwap.material = selected;

            // Update the mesh filter's vertices to the vertices' GameObjects' positions
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
            meshRebuilder.vertices[thisedge.vert1] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex1.transform.position - editingSpace.transform.position);

            meshRebuilder.vertices[thisedge.vert2] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex2.transform.position - editingSpace.transform.position);

            UpdateMesh(thisedge.id, thisedge.vert1, thisedge.vert2);

            Vector3 vertex1Pos = meshRebuilder.vertices[thisedge.vert1];
            Vector3 vertex2Pos = meshRebuilder.vertices[thisedge.vert2];

            // Continuously synchronize the position of the vertex without caching it until we release it
            EdgePullEvent edgeEvent = new EdgePullEvent
            {
                id = thisedge.id,
                meshId = meshRebuilder.id,
                vert1 = thisedge.vert1,
                vert2 = thisedge.vert2,
                position = thisedge.transform.position,
                vertex1Pos = vertex1Pos,
                vertex2Pos = vertex2Pos,
                isCached = false,
                released = false,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            NetworkMeshManager.instance.SynchronizeMeshEdgePull(edgeEvent);
        }
    }

    public void SetActiveEdges(Edge edge, bool active)
    {
        if (edge == null)
        {
            Debug.LogWarning("Warning: MoveEdge: Failed to call SetActiveEdges() on a null edge!");
            return;
        }

        foreach (Edge currEdge in meshRebuilder.edgeObjects)
        {
            if (currEdge == null || currEdge.id == edge.id || currEdge.GetComponent<MoveEdge>().isLocked) continue;

            currEdge.locked = !active;
            currEdge.GetComponent<MoveEdge>().materialSwap.material = (active) ? unselected : locked;
            currEdge.GetComponent<XRGrabInteractable>().enabled = active;
        }
    }

    // Update MeshFilter and re-draw in-game visuals
    public void UpdateMesh(int edgeId, int vertex1Id, int vertex2Id, bool skipThisEdgeId = true)
    {
        Vector3[] vertices = meshRebuilder.vertices;

        // Update actual mesh data
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        Transform vertex1Transform = meshRebuilder.vertexObjects[vertex1Id].transform;
        Transform vertex2Transform = meshRebuilder.vertexObjects[vertex2Id].transform;

        // Look through visuals Dictionary to update mesh visuals (reconnect edges to vertices)
        foreach (Edge edge in meshRebuilder.edgeObjects)
        {
            if (skipThisEdgeId && edge.id == edgeId) continue;

            GameObject edgeObject = edge.gameObject;
            int vert1 = edge.vert1;
            int vert2 = edge.vert2;

            // If either of the vertex values are the same as selectedVertex1, it will update the edges that vertex is connected to
            if (vert1 == vertex1Id || vert2 == vertex1Id)
            {
                // Set edge's position, scale, rotation to look at the vertices
                // (We only need to change the Y scale since that's the axis pointing up)
                edgeObject.transform.localPosition = ((vertices[vert1] + vertices[vert2]) / 2);
                Vector3 edgeScale = edgeObject.transform.localScale;
                edgeScale.y = (Vector3.Distance(vertices[vert1], vertices[vert2])) / 2;
                edgeObject.transform.localScale = edgeScale;
                edgeObject.transform.LookAt(vertex1Transform);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }

            // If either of the vertex values are the same as selectedVertex2, it will update the edges that vertex is connected to
            if (vert1 == vertex2Id || vert2 == vertex2Id)
            {
                // Set edge's position, scale, rotation to look at the vertices
                // (We only need to change the Y scale since that's the axis pointing up)
                edgeObject.transform.localPosition = ((vertices[vert1] + vertices[vert2]) / 2);
                Vector3 edgeScale = edgeObject.transform.localScale;
                edgeScale.y = (Vector3.Distance(vertices[vert1], vertices[vert2])) / 2;
                edgeObject.transform.localScale = edgeScale;
                edgeObject.transform.LookAt(vertex2Transform);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }
        }

        foreach (Face face in meshRebuilder.faceObjects)
        {
            GameObject faceObject = face.gameObject;
            int vert1 = face.vert1;
            int vert2 = face.vert2;
            int vert3 = face.vert3;

            // If either of the vertex values are the same as selectedVertex, it will update the edges that vertex is connected to
            if (vert1 == vertex1Id || vert2 == vertex1Id || vert3 == vertex1Id ||vert1 == vertex2Id || vert2 == vertex2Id || vert3 == vertex2Id)
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
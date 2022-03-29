using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using EasyMeshVR.Multiplayer;

public class MoveFace : MonoBehaviour
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

    // Edge lookup
    //Edge thisedge;

    Face thisFace;
    GameObject selectedFace;
    int selectedVertex1;
    int selectedVertex2;
    int selectedVertex3;

    Vertex vertex1;
    Vertex vertex2;
    Vertex vertex3;

    // int selectedEdge1;
    // int selectedEdge2;
    // int selectedEdge3;

    Edge edge1;
    Edge edge2;
    Edge edge3;
    
    public bool grabHeld = false;


    // Get all references we need and add control listeners
    void OnEnable()
    {
        // Get the editing model's MeshFilter
        meshRebuilder = transform.parent.GetComponent<MeshRebuilder>();
        model = meshRebuilder.model;
        mesh = model.GetComponent<MeshFilter>().mesh;
        thisFace = GetComponent<Face>();
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
        if (pulleyLocomotion.isMovingEditingSpace || thisFace.locked)
            return;

        //if(switchControllers.rayActive)
        materialSwap.material = hovered;

        // Keep mesh filter updated with most recent mesh data changes
        meshRebuilder.vertices = mesh.vertices;

                // Keep mesh filter updated with most recent mesh data changes
       // meshRebuilder.triangles = mesh.triangles;

        //print("Face " +thisFace.id + " vertices " + thisFace.vert1 + " " + thisFace.vert2 + " " + thisFace.vert3);

    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        if (thisFace.locked)
            return;

        materialSwap.material = unselected;
    }

    // Pull vertex to hand and update position on GameObject and in Mesh and change material
    void GrabPulled(SelectEnterEventArgs arg0)
    {
        if (pulleyLocomotion.isMovingEditingSpace || thisFace.locked)
            return;


        edge1 = meshRebuilder.edgeObjects[thisFace.edge1];
        edge2 = meshRebuilder.edgeObjects[thisFace.edge2];
        edge3 = meshRebuilder.edgeObjects[thisFace.edge3];

        SetActiveEdges(edge1, false);
        SetActiveEdges(edge2, false);
        SetActiveEdges(edge3, false);

        SetActiveFaces(thisFace, false);


        vertex1 = meshRebuilder.vertexObjects[thisFace.vert1];
        vertex2 = meshRebuilder.vertexObjects[thisFace.vert2];
        vertex3 = meshRebuilder.vertexObjects[thisFace.vert3];


        thisFace.transform.parent = model.transform;

        // Parent vertices and edges to face
        vertex1.transform.parent = thisFace.transform;
        vertex2.transform.parent = thisFace.transform;
        vertex3.transform.parent = thisFace.transform;

        edge1.transform.parent = thisFace.transform;
        edge2.transform.parent = thisFace.transform;
        edge3.transform.parent = thisFace.transform;

        

        vertex1.gameObject.SetActive(false);
        vertex2.gameObject.SetActive(false);
        vertex3.gameObject.SetActive(false);


        grabHeld = true;
        pulleyLocomotion.isMovingVertex = true;
    }

    // Stop updating the mesh data
    void GrabReleased(SelectExitEventArgs arg0)
    {
        if (thisFace.locked)
            return;

        SetActiveEdges(edge1, true);
        SetActiveEdges(edge2, true);
        SetActiveEdges(edge3, true);

        SetActiveFaces(thisFace, true);


        materialSwap.material = unselected;

        // Unparent the vertices from the edge
        vertex1.transform.parent = model.transform;
        vertex2.transform.parent = model.transform;
        vertex3.transform.parent = model.transform;

        edge1.transform.parent = model.transform;
        edge2.transform.parent = model.transform;
        edge3.transform.parent = model.transform;


        vertex1.gameObject.SetActive(true);
        vertex2.gameObject.SetActive(true);
        vertex3.gameObject.SetActive(true);


        grabHeld = false;

        Vector3 vertex1Pos = meshRebuilder.vertices[thisFace.vert1];
        Vector3 vertex2Pos = meshRebuilder.vertices[thisFace.vert2];
        Vector3 vertex3Pos = meshRebuilder.vertices[thisFace.vert3];

        // Synchronize the position of the mesh vertex by sending a cached event to other players
        FacePullEvent faceEvent = new FacePullEvent
        {
            id = thisFace.id,
            vert1 = thisFace.vert1,
            vert2 = thisFace.vert2,
            vert3 = thisFace.vert3,
            edge1 = thisFace.edge1,
            edge2 = thisFace.edge2,
            edge3 = thisFace.edge3,
            position = thisFace.transform.position,
            vertex1Pos = vertex1Pos,
            vertex2Pos = vertex2Pos,
            vertex3Pos = vertex3Pos,
            normal = thisFace.normal,
            isCached = true,
            released = true,
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
        };

        NetworkMeshManager.instance.SynchronizeMeshFacePull(faceEvent);

        // Update face position
        float totalX = vertex1Pos.x + vertex2Pos.x + vertex3Pos.x;
        float totalY = vertex1Pos.y + vertex2Pos.y + vertex3Pos.y;
        float totalZ = vertex1Pos.z + vertex2Pos.z + vertex3Pos.z;

        thisFace.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);


        pulleyLocomotion.isMovingVertex = false;

    }

    // If the grab button is held, keep updating mesh data until it's released
    void Update()
    {
        if (pulleyLocomotion.isMovingEditingSpace || thisFace.isHeldByOther || thisFace.locked)
        {
            grabInteractable.enabled = false;
            return;
        }
        grabInteractable.enabled = true;

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
            meshRebuilder.vertices[thisFace.vert1] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex1.transform.position - editingSpace.transform.position);

            meshRebuilder.vertices[thisFace.vert2] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex2.transform.position - editingSpace.transform.position);

            meshRebuilder.vertices[thisFace.vert3] = 
                Quaternion.Inverse(editingSpace.transform.rotation)
                * Vector3.Scale(inverseScale, vertex3.transform.position - editingSpace.transform.position);

            UpdateMesh(thisFace.vert1, thisFace.vert2, thisFace.vert3);

            Vector3 vertex1Pos = meshRebuilder.vertices[thisFace.vert1];
            Vector3 vertex2Pos = meshRebuilder.vertices[thisFace.vert2];
            Vector3 vertex3Pos = meshRebuilder.vertices[thisFace.vert3];

            // Continuously synchronize the face without caching it until we release it
            FacePullEvent faceEvent = new FacePullEvent
            {
                id = thisFace.id,
                vert1 = thisFace.vert1,
                vert2 = thisFace.vert2,
                vert3 = thisFace.vert3,
                edge1 = thisFace.edge1,
                edge2 = thisFace.edge2,
                edge3 = thisFace.edge3,
                position = thisFace.transform.position,
                vertex1Pos = vertex1Pos,
                vertex2Pos = vertex2Pos,
                vertex3Pos = vertex3Pos,
                normal = thisFace.normal,
                isCached = false,
                released = false,
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber
            };

            NetworkMeshManager.instance.SynchronizeMeshFacePull(faceEvent);
        }
    }

    public void SetActiveEdges(Edge edge, bool active)
    {
        foreach (Edge currEdge in meshRebuilder.edgeObjects)
        {
            if (currEdge.id == edge.id) continue;

            currEdge.locked = !active;
            currEdge.GetComponent<MoveEdge>().materialSwap.material = (active) ? unselected : locked;
            currEdge.GetComponent<XRGrabInteractable>().enabled = active;
        }
    }

    public void SetActiveFaces(Face face, bool active)
    {
        foreach (Face currFace in meshRebuilder.faceObjects)
        {
            if (currFace.id == face.id) continue;

            currFace.locked = !active;
            currFace.GetComponent<MoveFace>().materialSwap.material = (active) ? unselected : locked;
            currFace.GetComponent<XRGrabInteractable>().enabled = active;
        }
    }

    // Update MeshFilter and re-draw in-game visuals
    public void UpdateMesh(int vertex1Id, int vertex2Id, int vertex3Id, bool skipThisEdgeId = true)
    {
        Vector3[] vertices = meshRebuilder.vertices;

        // Update actual mesh data
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        Transform vertex1Transform = meshRebuilder.vertexObjects[vertex1Id].transform;
        Transform vertex2Transform = meshRebuilder.vertexObjects[vertex2Id].transform;
        Transform vertex3Transform = meshRebuilder.vertexObjects[vertex3Id].transform;

        // Look through visuals Dictionary to update mesh visuals (reconnect edges to vertices)
        foreach (Edge edge in meshRebuilder.edgeObjects)
        {
            if (skipThisEdgeId && (edge.id == thisFace.edge1 ||edge.id == thisFace.edge2 ||edge.id == thisFace.edge3)) continue;

            GameObject edgeObject = edge.gameObject;
            int vert1 = edge.vert1;
            int vert2 = edge.vert2;

            // If either of the vertex values are the same as selectedVertex1, it will update the edges that vertex is connected to
            if (vert1 == vertex1Id || vert2 == vertex1Id)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(vertex1Transform, Vector3.up);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }

            // If either of the vertex values are the same as selectedVertex2, it will update the edges that vertex is connected to
            if (vert1 == vertex2Id || vert2 == vertex2Id)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(vertex2Transform, Vector3.up);
                edgeObject.transform.rotation *= Quaternion.Euler(90, 0, 0);
            }

            // If either of the vertex values are the same as selectedVertex2, it will update the edges that vertex is connected to
            if (vert1 == vertex3Id || vert2 == vertex3Id)
            {
                // Set the edge's position to between the two vertices and scale it appropriately
                float edgeDistance = 0.5f * Vector3.Distance(vertices[edge.vert1], vertices[edge.vert2]);
                edgeObject.transform.localPosition = (vertices[vert1] + vertices[vert2]) / 2;
                edgeObject.transform.localScale = new Vector3(edgeObject.transform.localScale.x, edgeDistance, edgeObject.transform.localScale.z);

                // Orient the edge to look at the vertices (specifically the one we're currently holding)
                edgeObject.transform.LookAt(vertex3Transform, Vector3.up);
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
            if (vert1 == vertex1Id || vert2 == vertex1Id || vert3 == vertex1Id ||vert1 == vertex2Id || vert2 == vertex2Id || vert3 == vertex2Id ||
             vert1 == vertex3Id || vert2 == vertex3Id || vert3 == vertex3Id)
            {
                float totalX = vertices[vert1].x + vertices[vert2].x + vertices[vert3].x;
                float totalY = vertices[vert1].y + vertices[vert2].y + vertices[vert3].y;
                float totalZ = vertices[vert1].z + vertices[vert2].z + vertices[vert3].z;

                faceObject.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using EasyMeshVR.Multiplayer;

// just for changing material, can be converted into moveFace later
// need something in either moveVertices or moveEdge to move Face handle if one of its vertices moves
public class HoverFace : MonoBehaviour
{
    [SerializeField] XRGrabInteractable grabInteractable;
   // [SerializeField] LockVertex lockVertex;

    [SerializeField] Material unselected;   // gray
    [SerializeField] Material hovered;      // orange
    [SerializeField] Material selected;     // light blue

    MeshRenderer materialSwap;

    Face thisFace;
    int selectedFace;

        void OnEnable()
    {
        thisFace = GetComponent<Face>();

        // Get the vertex GameObject material
        materialSwap = GetComponent<MeshRenderer>();

        // Hover listeners to change vertex color
        grabInteractable.hoverEntered.AddListener(HoverOver);
        grabInteractable.hoverExited.AddListener(HoverExit);
    }

    // We don't need the control listeners if OnDisable() is ever called
    void OnDisable()
    {
        grabInteractable.hoverEntered.RemoveListener(HoverOver);
        grabInteractable.hoverExited.RemoveListener(HoverExit);
    }

    // Get original position of Vertex before moving
    // Set material to Selected (change name to hover)
    void HoverOver(HoverEnterEventArgs arg0)
    {
        materialSwap.material = hovered;

        // Keep mesh filter updated with most recent mesh data changes
       // MeshRebuilder.instance.vertices = mesh.vertices;

        // The selected vertex is just the saved id of this vertex representing its index in the vertices array
        selectedFace = thisFace.id;
    }

    // Set material back to Unselected
    void HoverExit(HoverExitEventArgs arg0)
    {
        materialSwap.material = unselected;
    }

}

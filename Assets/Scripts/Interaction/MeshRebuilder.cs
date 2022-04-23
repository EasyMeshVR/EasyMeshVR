using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using EasyMeshVR.Core;

public class MeshRebuilder : MonoBehaviour
{
    public int id = 0;
    public bool isInitialized = false;

    [SerializeField]
    public GameObject editingSpace;
    public GameObject model;
    
    // Holds the vertex and edge prefabs
    public GameObject vertex;
    public GameObject edge;
    public GameObject face;

    GameObject newVertex;
    GameObject newEdge;

    // Mesh data
    Mesh mesh;
    public Vector3[] vertices;
    Vector3 vertexPosition;
    public int[] triangles;

    // Stores the vertex/edge visual data, i.e. which edges are connected to which vertices
    // Mostly accessed in MoveVertices.cs (and eventually MoveEdges.cs)
    // public static Dictionary<GameObject, List<int>> visuals;
    public List<Edge> edgeObjects;
    public List<Vertex> vertexObjects;
    public List<Face> faceObjects;

    // Setup
    void Start()
    {
        Debug.Log("In MeshRebuilder:Start() - GameObject: " + name);

        if (!isInitialized)
        {
            Debug.Log("In MeshRebuilder:Start() - Initializing GameObject: " + name);
            Initialize();
        }
    }

    public void Initialize()
    {
        Debug.Log("In MeshRebuilder:Initialize() - GameObject: " + name);

        isInitialized = true;

        edgeObjects = new List<Edge>();
        vertexObjects = new List<Vertex>();
        faceObjects = new List<Face>();

        editingSpace = GameObject.FindGameObjectWithTag(Constants.EDITING_SPACE_TAG);

        // For importing in real time we would need the script to get the model automatically
        model = gameObject;
        model.tag = ("Model");

        // Copy vertices and triangles
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Start visualizing the mesh
        RemoveDuplicates();
        CreateVisuals();
    }

    // Deletes the duplicate vertices Unity and STL files create
    // Re-references those duplicate vertices in the triangles array with unique ones only
    public void RemoveDuplicates()
    {
        // Filter out unique vertices and triangles, and store indices of every duplicate of a vertex (2 or more dupes)
        HashSet<Vector3> vertexUnique = new HashSet<Vector3>();
        Dictionary<List<int>, Vector3> vertexDuplicate = new Dictionary<List<int>, Vector3>();
        List<int> triangleUnique = new List<int>();

        // Loop over the vertices array, separating duplicates and uniques
        for (int i = 0; i < vertices.Length; i++)
        {
            // List for each index to add 
            List<int> dupeVert = new List<int>();

            // If the hashset already has the vertex, it's a duplicate
            if (vertexUnique.Contains(vertices[i]))
            {
                // If this is not the first duplicate of the vertex, get the previous list, remove the entry, add new index, readd entry
                if (vertexDuplicate.ContainsValue(vertices[i]))
                {
                    List<int> indicies = vertexDuplicate.FirstOrDefault(x => x.Value == vertices[i]).Key;
                    vertexDuplicate.Remove(indicies);
                    indicies.Add(i);
                    vertexDuplicate.Add(indicies, vertices[i]);
                }
                dupeVert.Add(i);
                vertexDuplicate.Add(dupeVert, vertices[i]);
            }
            else
            {
                vertexUnique.Add(vertices[i]);
            }
        }

        // Loop over the triangles array
        for (int i = 0; i < triangles.Length; i++)
        {
            // Check if vertex in triangles array is a duplicate, replace with original if it is
            if (vertexDuplicate.ContainsValue(vertices[triangles[i]]))
            {
                // Need to loop through vertexUnique for imported meshes, for meshes in Unity the first set of vertices are unique
                int j = 0;
                foreach (Vector3 vertex in vertexUnique)
                {
                    if (vertex == vertices[triangles[i]])
                    {
                        triangleUnique.Add(j);
                        break;
                    }
                    j++;
                }
            }
            else
            {
                triangleUnique.Add(triangles[i]);
            }
        }

        // Copy unique vertices to array
        Vector3[] newVertices = new Vector3[vertexUnique.Count];
        int v = 0;
        foreach (Vector3 vertex in vertexUnique)
            newVertices[v++] = vertex;
        vertices = newVertices;

        // Copy unique triangles to array
        int[] newTriangles = new int[triangleUnique.Count];
        int t = 0;
        foreach (int triangle in triangleUnique)
            newTriangles[t++] = triangle;
        triangles = newTriangles;

        // Update the mesh filter with new unique vertex and triangle data
        mesh.Clear();
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.RecalculateNormals();
    }

    // Actually create the vertex and edge GameObject interactables
    public void CreateVisuals()
    {
        int edgeCount = 0;
        int faceCount = 0;

        // Repeats for every vertex stored in the mesh filter
        for (int i = 0; i < vertices.Length; i++)
        {
            // Create a new vertex from a prefab, make it a child of the mesh and set it's position
            GameObject newVertex = Instantiate(vertex, model.transform);
            newVertex.transform.localPosition = vertices[i];
            newVertex.name = "Vertex" + i.ToString();

            // Set the id of the Vertex component to be the index in the vertices array
            Vertex vertexObj = newVertex.GetComponent<Vertex>();
            vertexObj.id = i;
            vertexObj.thisVertex = newVertex;
            vertexObjects.Add(vertexObj);

            if (!ToolManager.instance.grabVertex)
            {
                newVertex.SetActive(false);
            }

            // Save vertices adjacent to the one we're currently looking at (no duplicates)
            HashSet<int> adjacentVertices = new HashSet<int>();

            // Loop through the triangles array and look for the adjacent vertices
            for (int j = 0; j < triangles.Length; j += 3)
            {
                // Triangles are created in triplets
                // Entering "0, 1, 2," in the triangles array would make a triangle

                if (triangles[j] == i) // First index of triplet
                {
                    adjacentVertices.Add(triangles[j + 1]);
                    adjacentVertices.Add(triangles[j + 2]);
                }
                else if (triangles[j + 1] == i) // Second index of triplet
                {
                    adjacentVertices.Add(triangles[j]);
                    adjacentVertices.Add(triangles[j + 2]);
                }
                else if (triangles[j + 2] == i) // Third index of triplet
                {
                    adjacentVertices.Add(triangles[j]);
                    adjacentVertices.Add(triangles[j + 1]);
                }
            }

            // Connect a line from our starting vertex to each adjacent vertex
            foreach (int k in adjacentVertices)
            {
                // Ignore adjacent vertices we've already dealt with
                if (k < i)
                    continue;

                // Same as vertex, create a new edge object and set its parent
                newEdge = Instantiate(edge, model.transform);
                newEdge.name = "Edge" + (i + edgeCount++).ToString();

                // Set edge's position, scale, rotation to look at the vertices
                // (We only need to change the Y scale since that's the axis pointing up)
                newEdge.transform.localPosition = ((vertices[i] + vertices[k]) / 2);
                Vector3 edgeScale = newEdge.transform.localScale;
                edgeScale.y = (Vector3.Distance(vertices[i], vertices[k])) / 2;
                newEdge.transform.localScale = edgeScale;
                newEdge.transform.LookAt(newVertex.transform);
                newEdge.transform.rotation *= Quaternion.Euler(90, 0, 0);

                // Add edge and it's connecting vertices to a dictionary reference for use in other scripts
                Edge edgeComponent = newEdge.GetComponent<Edge>();
                edgeComponent.id = edgeObjects.Count();
                edgeComponent.vert1 = i;
                edgeComponent.vert2 = k;
                edgeComponent.thisEdge = newEdge;
                edgeObjects.Add(edgeComponent);

                if (!ToolManager.instance.grabEdge)
                {
                    newEdge.SetActive(false);
                }
            }

            // Add Edge id to Vertex component (used in Merge tool)
            foreach (Edge edge in edgeObjects)
            {
                if (edge.vert1 == i || edge.vert2 == i)
                    vertexObj.connectedEdges.Add(edge);
            }

            edgeCount--;
        }

        // Triangle handles
        for(int i = 0; i < triangles.Length; i+=3)
        {
            GameObject newFace = Instantiate(face, model.transform);
            newFace.name = "Face" + (faceCount++).ToString();

            // Add face to list and get vertices
            Face faceComponent = newFace.GetComponent<Face>();
            faceComponent.id = faceObjects.Count();
            faceComponent.vert1 = triangles[i];
            faceComponent.vert2 = triangles[i+1];
            faceComponent.vert3 = triangles[i+2];

            faceComponent.vertObj1 = vertexObjects[faceComponent.vert1];
            faceComponent.vertObj2 = vertexObjects[faceComponent.vert2];
            faceComponent.vertObj3 = vertexObjects[faceComponent.vert3];


            // Store face normal
            Vector3 e1 = vertices[faceComponent.vert2] - vertices[faceComponent.vert1];
            Vector3 e2 = vertices[faceComponent.vert3] - vertices[faceComponent.vert2];
            faceComponent.normal = Vector3.Normalize(Vector3.Cross(e1,e2));

            // Place face object in center of triangle
            float totalX = vertices[faceComponent.vert1].x + vertices[faceComponent.vert2].x + vertices[faceComponent.vert3].x;
            float totalY = vertices[faceComponent.vert1].y + vertices[faceComponent.vert2].y + vertices[faceComponent.vert3].y;
            float totalZ = vertices[faceComponent.vert1].z + vertices[faceComponent.vert2].z + vertices[faceComponent.vert3].z;

            // Place faceComponent in Vertex object list (Vertex.cs, used in Merge.cs)
            foreach (Vertex vertex in vertexObjects)
            {
                if (vertex.id == faceComponent.vert1 || vertex.id == faceComponent.vert2 || vertex.id == faceComponent.vert3)
                    vertex.connectedFaces.Add(faceComponent);
            }

            // Store edge
            foreach(Edge edge in edgeObjects)
            {
                if((edge.vert1 == faceComponent.vert1 && edge.vert2 == faceComponent.vert2) || (edge.vert2 == faceComponent.vert1 && edge.vert1 == faceComponent.vert2))
                {
                    faceComponent.edge1 = edge.id;
                    faceComponent.edgeObj1 = edgeObjects[edge.id];
                }
                if((edge.vert1 == faceComponent.vert2 && edge.vert2 == faceComponent.vert3) || (edge.vert2 == faceComponent.vert2 && edge.vert1 == faceComponent.vert3))
                {
                    faceComponent.edge2 = edge.id;
                    faceComponent.edgeObj2 = edgeObjects[edge.id];

                }
                if((edge.vert1 == faceComponent.vert1 && edge.vert2 == faceComponent.vert3) || (edge.vert2 == faceComponent.vert1 && edge.vert1 == faceComponent.vert3))
                {
                    faceComponent.edge3 = edge.id;
                    faceComponent.edgeObj3 = edgeObjects[edge.id];
                }
            }
            newFace.transform.localPosition = new Vector3(totalX/3, totalY/3, totalZ/3);

            faceComponent.thisFace = newFace;
            faceObjects.Add(faceComponent);

            if (!ToolManager.instance.grabFace)
            {
                newFace.SetActive(false);
            }
        }
    }

    public void removeVisuals()
    {
         foreach (Transform child in transform) 
            GameObject.Destroy(child.gameObject);

        edgeObjects.Clear();
        vertexObjects.Clear();
        faceObjects.Clear();
    }

    public void ClearHeldDataForPlayer(Player player)
    {
        Debug.LogFormat("Called ClearHeldDataForPlayer for player Name: {0} ActorNumber: {1}", player.NickName, player.ActorNumber);

        // We start a coroutine since this may be a long running process
        // and we don't want to hang the main thread
        StartCoroutine(ClearHeldData(player));
    }

    private IEnumerator ClearHeldData(Player player)
    {
        foreach (Vertex vertexObj in vertexObjects)
        {
            if (vertexObj.isHeldByOther && vertexObj.heldByActorNumber == player.ActorNumber)
            {
                Debug.LogFormat("Cleared vertex id: {0} held by player Name: {1} ActorNumber: {2}", vertexObj.id, player.NickName, player.ActorNumber);
                vertexObj.isHeldByOther = false;
                vertexObj.heldByActorNumber = -1;
            }

            yield return null;
        }

        foreach (Edge edgeObj in edgeObjects)
        {
            if (edgeObj.isHeldByOther && edgeObj.heldByActorNumber == player.ActorNumber)
            {
                Debug.LogFormat("Cleared edge id: {0} held by player Name: {1} ActorNumber: {2}", edgeObj.id, player.NickName, player.ActorNumber);
                edgeObj.isHeldByOther = false;
                edgeObj.heldByActorNumber = -1;
                edgeObj.locked = false;
                edgeObj.GetComponent<MoveEdge>().SetActiveEdges(edgeObj, true);
            }

            yield return null;
        }

        foreach (Face faceObj in faceObjects)
        {
            if (faceObj.isHeldByOther && faceObj.heldByActorNumber == player.ActorNumber)
            {
                Debug.LogFormat("Cleared face id: {0} held by player Name: {1} ActorNumber: {2}", faceObj.id, player.NickName, player.ActorNumber);
                faceObj.isHeldByOther = false;
                faceObj.heldByActorNumber = -1;
                faceObj.locked = false;
                faceObj.GetComponent<MoveFace>().SetActiveFaces(faceObj, true);
            }

            yield return null;
        }
    }
}

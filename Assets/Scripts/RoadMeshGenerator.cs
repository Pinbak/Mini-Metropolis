using System;
using System.Collections.Generic;
using Junctions;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RoadMeshGenerator : MonoBehaviour
{
    [SerializeField] private float meshVertexSize = 0f;
    [SerializeField] private float diagonalRoadLength = .7071f;
    [SerializeField] private float straightRoadLength = .5f;
    [SerializeField] private float globalRoadWidth = .3f;
    [SerializeField] private float capLength = .3f;
    [SerializeField] private float curviness = .3f;
    [SerializeField] private float cornerLength = .3f;
    [Range(.1f, .5f)]
    [SerializeField] private float meshResolution = .3f;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private bool regenerateMesh;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private void Start()
    {
        placementManager.FinishedBuildingRoads += HandleMeshGeneration;
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void Update()
    {
        if (regenerateMesh)
            RegenerateMeshes();
        regenerateMesh = false;
    }

    private void HandleMeshGeneration(Node node)
    {
        throw new NotImplementedException();
    }

    private void RegenerateMeshes()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        // var currentVertex = 0
        for (var x = 0; x < gridManager.Width; x++)
        for (var y = 0; y < gridManager.Height; y++)
        {
            var node = gridManager.Grid[x, y];
            if (node.Type is NodeType.Empty) continue;
            if (node.Neighbours.Count == 0) continue;

            var nodeMesh = new NodeMesh(node, gridManager);
            var tris = nodeMesh.Triangles;
            
            var startingIndex = vertices.Count;
            vertices.Add(tris[0].A1); // add the centre point

            foreach (var tri in tris)
            {
                var localStartingIndex = vertices.Count; // starting index of specific local tri
                vertices.Add(tri.A2); // add the left point
                vertices.Add(tri.A3); // add the right point
                
                triangles.Add(startingIndex);
                triangles.Add(localStartingIndex);
                triangles.Add(localStartingIndex + 1);
            }
        }
        _mesh.Clear();
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();
    }
    
    private void OnDrawGizmos()
    {
        if (!gridManager.GridExists()) return;
        Gizmos.color = Color.red;
        
        for (var x = 0; x < gridManager.Width; x++)
        for (var y = 0; y < gridManager.Height; y++)
        {
            var node = gridManager.Grid[x, y];
            if (node.Type is NodeType.Empty) continue;
            if (node.Neighbours.Count == 0) continue;
            var position = gridManager.GridToWorld(new Vector2Int(x, y));
            // Gizmos.DrawSphere(position, meshVertexSize);
            var nodeMesh = new NodeMesh(node, gridManager);
            var triangles = nodeMesh.Triangles;
            foreach (var nodeNeighbour in node.Neighbours)
            {
                var neighbourPosition = gridManager.GridToWorld(new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y));
                // Gizmos.DrawLine(position, neighbourPosition);
            }

            Gizmos.color = nodeMesh.Type switch
            {
                JunctionType.DeadEnd => Color.green,
                JunctionType.Straight => Color.red,
                JunctionType.AcuteCorner => Color.blue,
                JunctionType.RightAngleCorner => Color.lightBlue,
                JunctionType.ObtuseCorner => Color.cadetBlue,
                JunctionType.Complex => Color.purple,
                _ => Gizmos.color
            };
            Gizmos.DrawSphere(position, meshVertexSize);
            
            foreach (var triangle in triangles)
            {
                // Gizmos.DrawSphere(triangle, meshVertexSize);
                Gizmos.DrawLine(triangle.A1, triangle.A2);
                Gizmos.DrawLine(triangle.A1, triangle.A3);
                Gizmos.DrawLine(triangle.A2, triangle.A3);
                // Gizmos.color = Color.green;
                // Gizmos.DrawSphere(triangle.A2, meshVertexSize);
                // Gizmos.color = Color.blue;
                // Gizmos.DrawSphere(triangle.A3, meshVertexSize);
                // Gizmos.color = Color.red;
            }
        }
    }
}
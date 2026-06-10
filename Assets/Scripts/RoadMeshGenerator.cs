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

    private List<Triangle> CalculateMeshPoints(Node node)
    {
        var nodeCentre = new Vector2Int(node.X, node.Y);
        Vector3 nodeCentreInWorld = gridManager.GridToWorld(nodeCentre);
        var tris = new List<Triangle>();
        foreach (var neighbour in node.Neighbours)
        {
            var neighbourPosition = new Vector2Int(neighbour.X, neighbour.Y);
            Vector3 neighbourWorldPosition = gridManager.GridToWorld(neighbourPosition);
            var direction = neighbourWorldPosition - nodeCentreInWorld;
            direction.Normalize();
            var isDiagonal = direction.x != 0 && direction.z != 0;
            var roadLength = isDiagonal ? diagonalRoadLength : straightRoadLength;
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var left = nodeCentreInWorld - perpendicular * (globalRoadWidth * .5f);
            left = direction * roadLength + left;
            var right = nodeCentreInWorld + perpendicular * (globalRoadWidth * .5f);
            right = direction * roadLength + right;
            tris.Add(new Triangle(nodeCentreInWorld, left, right));
        }
        
        tris.Sort((a, b) =>
        {
            var aDir = a.Centre - nodeCentreInWorld;
            var bDir = b.Centre - nodeCentreInWorld;
            
            var angleA = Vector3.SignedAngle(nodeCentreInWorld.normalized, aDir.normalized, Vector3.up);
            var angleB = Vector3.SignedAngle(nodeCentreInWorld.normalized, bDir.normalized, Vector3.up);

            if (angleA > angleB)
                return 1;
            if (angleA < angleB)
                return -1;
            return 0;
        });
        
        if (tris.Count == 2)
        {
            var averagePosition = new Vector3();
            foreach (var triangle in tris)
            {
                averagePosition += triangle.Centre;
            }
            averagePosition /= tris.Count;
            
            // not a straight piece or 8 connections
            if (averagePosition != nodeCentreInWorld)
            {
                var direction = averagePosition - nodeCentreInWorld;
                direction.Normalize();
                direction *= -1;
                var cornerPosition = nodeCentreInWorld + direction * cornerLength;
                var angle = Vector3.SignedAngle(nodeCentreInWorld.normalized, direction, Vector3.up);

                var insertIndex = 0;
                while(insertIndex < tris.Count)
                {
                    var triDirection = tris[insertIndex].Centre - nodeCentreInWorld;
                    var triAngle = Vector3.SignedAngle(nodeCentreInWorld.normalized, triDirection.normalized,
                        Vector3.up);
                    if (angle < triAngle)
                        break;
                    insertIndex++;
                }

                var previous = (insertIndex - 1 + tris.Count) % tris.Count;
                var next = insertIndex % tris.Count;

                var prevTri = tris[previous];
                var nextTri = tris[next];

                var bezierPoints = new List<Vector3>();
                for (var t = meshResolution; t < 1f; t += meshResolution)
                {
                    var point = BezierCurve.EvaluateQuadratic(prevTri.A3, cornerPosition, nextTri.A2, t);
                    bezierPoints.Add(point);
                }
                
                for (var i = 0; i < bezierPoints.Count; i++)
                {
                    // start of the bezier
                    if (i == 0)
                    {
                        tris.Add(new Triangle(nodeCentreInWorld, prevTri.A3, bezierPoints[i]));
                    }
                    // all points in the bezier
                    if (i != bezierPoints.Count - 1)
                    {
                        tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[i], bezierPoints[i + 1]));
                    }
                    // last point of the bezier
                    else if (i == bezierPoints.Count - 1)
                    {
                        tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[i], nextTri.A2));
                    }
                }
            }
            
        }
        
        tris.Sort((a, b) =>
        {
            var aDir = a.Centre - nodeCentreInWorld;
            var bDir = b.Centre - nodeCentreInWorld;
            
            var angleA = Vector3.SignedAngle(nodeCentreInWorld.normalized, aDir.normalized, Vector3.up);
            var angleB = Vector3.SignedAngle(nodeCentreInWorld.normalized, bDir.normalized, Vector3.up);

            if (angleA > angleB)
                return 1;
            if (angleA < angleB)
                return -1;
            return 0;
        });
        
        // it's not possible to have a road with no neighbours
        // if we are at a dead-end
        if (tris.Count == 1)
        {
            // todo repeating the same as from above
            var neighbourPosition = new Vector2Int(node.Neighbours[0].X, node.Neighbours[0].Y);
            Vector3 neighbourWorldPosition = gridManager.GridToWorld(neighbourPosition);
            var direction = neighbourWorldPosition - nodeCentreInWorld;
            direction.Normalize();
            direction *= -1; // flip the direction
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var a = nodeCentreInWorld - perpendicular * (globalRoadWidth * .5f);
            var b = nodeCentreInWorld + perpendicular * (globalRoadWidth * .5f);
            var c = nodeCentreInWorld + direction * capLength;

            var bezierPoints = new List<Vector3>();
            for (var t = meshResolution; t < 1f; t += meshResolution)
            {
                var point = BezierCurve.EvaluateQuadratic(a, c, b, t);
                bezierPoints.Add(point);
            }

            for (var i = 0; i < bezierPoints.Count; i++)
            {
                // start of the bezier
                if (i == 0)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, a, bezierPoints[i]));
                }
                // all points in the bezier
                if (i != bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[i], bezierPoints[i + 1]));
                }
                // last point of the bezier
                else if (i == bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[i], b));
                }
            }
        }
        
        // filling in the gaps
        var numberOfTriangles = tris.Count;
        var gapFillTris = new List<Triangle>();
        for (var i = 0; i < numberOfTriangles; i++)
        {
            var a = tris[i].A3;
            var b = tris[(i + 1) % numberOfTriangles].A2;
            var bezierPoints = new List<Vector3>();
            for (var t = meshResolution; t < 1f; t += meshResolution)
            {
                var movedCentre = Vector3.Lerp((a + b) / 2, nodeCentreInWorld, curviness);
                var point = BezierCurve.EvaluateQuadratic(a, movedCentre, b, t);
                bezierPoints.Add(point); 
                // Gizmos.DrawSphere(point, .05f);
            }
            
            for (var j = 0; j < bezierPoints.Count; j++)
            {
                // start of the bezier
                if (j == 0)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, a, bezierPoints[j]));
                }
                // all points in the bezier
                if (j != bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[j], bezierPoints[j + 1]));
                }
                // last point of the bezier
                else if (j == bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(nodeCentreInWorld, bezierPoints[j], b));
                }
            }
            
            
            // gapFillTris.Add(new Triangle(nodeCentreInWorld, a, b)); // todo needed still for straight roads
        }
        

        tris.AddRange(gapFillTris);
        return tris;
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

            var junction = new Junction(node, gridManager);
            var tris = junction.Triangles;
            
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
            var triangles = CalculateMeshPoints(node);
            foreach (var nodeNeighbour in node.Neighbours)
            {
                var neighbourPosition = gridManager.GridToWorld(new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y));
                // Gizmos.DrawLine(position, neighbourPosition);
            }
            
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
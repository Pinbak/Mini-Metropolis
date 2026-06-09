using System.Collections.Generic;
using UnityEngine;

public class RoadMeshGenerator : MonoBehaviour
{
    [SerializeField] private float meshVertexSize = 0f;
    [SerializeField] private float diagonalRoadLength = .7071f;
    [SerializeField] private float straightRoadLength = .5f;
    [SerializeField] private float globalRoadWidth = .3f;
    [SerializeField] private GridManager gridManager;
    
    public List<Triangle> CalculateMeshPoints(Node node, float roadWidth, float resolution = 0.1f)
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
            var left = nodeCentreInWorld - perpendicular * (roadWidth * .5f);
            left = direction * roadLength + left;
            var right = nodeCentreInWorld + perpendicular * (roadWidth * .5f);
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

        if (tris.Count <= 1) return tris;

        var numberOfTriangles = tris.Count;
        var gapFillTris = new List<Triangle>();
        for (var i = 0; i < numberOfTriangles; i++)
        {
            gapFillTris.Add(new Triangle(nodeCentreInWorld, tris[(i + 1) % numberOfTriangles].A2, tris[i].A3));
        }

        tris.AddRange(gapFillTris);
        return tris;
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
            var triangles = CalculateMeshPoints(node, globalRoadWidth);
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
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(triangle.A2, meshVertexSize);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(triangle.A3, meshVertexSize);
                Gizmos.color = Color.red;
            }
        }
    }
}
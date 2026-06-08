using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject roadStructure;
    [SerializeField] private float meshVertexSize = .2f;
    [SerializeField] private float diagonalRoadLength = .7071f;
    [SerializeField] private float straightRoadLength = .5f;
    [SerializeField] private float globalRoadWidth = .5f;
    private int _offsetX;
    private int _offsetY;
    private Grid _grid;
    private GameObject _startingNode;
    private Vector3Int _startingPosition;
    private Vector3Int _lastSuccessfulPosition;
    private List<(int x, int y)> _validNeighbourNodes = new();
    
    private void Start()
    {
        _grid = new Grid(width, height);
        _offsetX = width / 2;
        _offsetY = height / 2;
    }

    private Vector2Int WorldToGrid(Vector3Int worldPosition)
    {
        return new Vector2Int(
            worldPosition.x + _offsetX,
            worldPosition.z + _offsetY
            );
    }

    private Vector3Int GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3Int(
            gridPosition.x - _offsetX,
            0,
            gridPosition.y - _offsetY);
    }

    public void StartRoadPlacement(Vector3Int position)
    {
        if (!IsPositionInBound(position)) return;
        if (!IsPositionFree(position)) return;
        
        RemoveStartingNode();

        var gridPosition = WorldToGrid(position);
        _validNeighbourNodes = _grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
        PlaceStartingNode(position);
    }

    public void MouseDown(Vector3 position)
    {
        var distance = Vector3.Distance(_startingPosition, position);
        if (!(distance > 1)) return;
        
        var direction = position - _startingPosition;
        direction.Normalize();
        var targetPosition = _startingPosition + new Vector3Int(
            Mathf.RoundToInt(direction.x), 0, Mathf.RoundToInt(direction.z));

        var gridTargetPosition = WorldToGrid(targetPosition);

        foreach (var validNeighbourNode in _validNeighbourNodes)
        {
            if (validNeighbourNode != (gridTargetPosition.x, gridTargetPosition.y)) continue;
            _lastSuccessfulPosition = targetPosition;
            EndRoadPlacement(gridTargetPosition);
            return;
        }

    }

    private void EndRoadPlacement(Vector2Int endGridPosition)
    {
        // only gets called when the final placement is valid
        var startGridPosition =
            WorldToGrid(new Vector3Int(_startingPosition.x, _startingPosition.y, _startingPosition.z));

        var startNode = _grid[startGridPosition.x, startGridPosition.y];
        var endNode = _grid[endGridPosition.x, endGridPosition.y];

        // change to road if not already
        if (startNode.Type is NodeType.Empty) startNode.Type = NodeType.Road;
        if (endNode.Type is NodeType.Empty) endNode.Type = NodeType.Road;
        
        // add the neighbours for the connection
        startNode.Neighbours.Add(endNode);
        endNode.Neighbours.Add(startNode);
        
        Instantiate(roadStructure, _startingPosition, Quaternion.identity);
        Instantiate(roadStructure, _lastSuccessfulPosition, Quaternion.identity);
        
        // RemovePlanning();
        StartRoadPlacement(_lastSuccessfulPosition);
    }

    private void PlaceStartingNode(Vector3Int position)
    {
        _startingNode = Instantiate(roadStructure, position, Quaternion.identity);
        _startingPosition = position;
        _lastSuccessfulPosition = position;
    }

    private bool IsPositionFree(Vector3Int position)
    {
        var gridPosition = WorldToGrid(position);
        return _grid[gridPosition.x, gridPosition.y].Type == NodeType.Empty ||
               _grid[gridPosition.x, gridPosition.y].Type == NodeType.Road;
    }

    private bool IsPositionInBound(Vector3Int position)
    {
        var gridPosition = WorldToGrid(position);
        return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
    }

    public void RemoveStartingNode()
    {
        if (_startingNode is null) return;
        Destroy(_startingNode.gameObject);
        _startingNode = null;
    }
    
    // todo move
    public List<Triangle> CalculateMeshPoints(Node node, float roadWidth, float resolution = 0.1f)
    {
        var nodeCentre = new Vector2Int(node.X, node.Y);
        Vector3 nodeCentreInWorld = GridToWorld(nodeCentre);
        var points = new List<Vector3>();
        var tris = new List<Triangle>();
        foreach (var neighbour in node.Neighbours)
        {
            var neighbourPosition = new Vector2Int(neighbour.X, neighbour.Y);
            Vector3 neighbourWorldPosition = GridToWorld(neighbourPosition);
            var direction = neighbourWorldPosition - nodeCentreInWorld;
            direction.Normalize();
            var isDiagonal = direction.x != 0 && direction.z != 0;
            var roadLength = isDiagonal ? diagonalRoadLength : straightRoadLength;
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var left = nodeCentreInWorld - perpendicular * (roadWidth * .5f);
            left = direction * roadLength + left;
            var right = nodeCentreInWorld + perpendicular * (roadWidth * .5f);
            right = direction * roadLength + right;
            points.Add(left);
            points.Add(right);
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
        if (_grid is null) return;
        Gizmos.color = Color.red;
        
        for (var x = 0; x < 10; x++)
        for (var y = 0; y < 10; y++)
        {
            var node = _grid[x, y];
            if (node.Type is NodeType.Empty) continue;
            if (node.Neighbours.Count == 0) continue;
            var position = GridToWorld(new Vector2Int(x, y));
            // Gizmos.DrawSphere(position, meshVertexSize);
            var triangles = CalculateMeshPoints(node, globalRoadWidth);
            foreach (var nodeNeighbour in node.Neighbours)
            {
                var neighbourPosition = GridToWorld(new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y));
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
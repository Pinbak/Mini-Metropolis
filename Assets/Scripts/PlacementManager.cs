using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject roadStructure;
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
            Gizmos.DrawSphere(position, 0.2f);
            foreach (var nodeNeighbour in node.Neighbours)
            {
                var neighbourPosition = GridToWorld(new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y));
                Gizmos.DrawLine(position, neighbourPosition);
            }
        }
    }
}
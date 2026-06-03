using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject roadStructure;
    [SerializeField] private GameObject potentialPlacementIndicator;
    private int _offsetX;
    private int _offsetY;
    private Grid _grid;
    private GameObject _startingNode;
    private List<(int x, int y)> _validNeighbourNodes = new();
    private readonly List<GameObject> _placementIndicators = new();
    private Vector3Int _currentMousePosition;
    
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

        RemoveIndicators();
        
        var gridPosition = WorldToGrid(position);
        _validNeighbourNodes = _grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
        PlaceStartingNode(position);

        foreach (var validNeighbourNode in _validNeighbourNodes)
        {
            var indicatorPosition = GridToWorld(new Vector2Int(validNeighbourNode.x, validNeighbourNode.y));
            var placementIndicator = Instantiate(potentialPlacementIndicator, indicatorPosition, Quaternion.identity);
            var placementHelper = placementIndicator.GetComponent<PlacementHelper>();
            placementHelper.HoverEnter += EndRoadPlacement;
            _placementIndicators.Add(placementIndicator);
        }
    }

    private void RemoveIndicators()
    {
        foreach (var placementIndicator in _placementIndicators)
        {
            var placementHelper = placementIndicator.GetComponent<PlacementHelper>();
            placementHelper.HoverEnter -= EndRoadPlacement; // todo garbage collector might be doing this anyway
            Destroy(placementIndicator.gameObject);
        }
        _placementIndicators.Clear();
    }

    public void MouseDown(Vector3Int position)
    {
        _currentMousePosition = position;
    }

    private void EndRoadPlacement()
    {
        // only gets called when the final placement is valid

        var startGridPosition = WorldToGrid(new Vector3Int((int)_startingNode.transform.position.x,
            (int)_startingNode.transform.position.y, (int)_startingNode.transform.position.z));
        var endGridPosition = WorldToGrid(_currentMousePosition);

        var startNode = _grid[startGridPosition.x, startGridPosition.y];
        var endNode = _grid[endGridPosition.x, endGridPosition.y];

        // change to road if not already
        if (startNode.Type is NodeType.Empty) startNode.Type = NodeType.Road;
        if (endNode.Type is NodeType.Empty) endNode.Type = NodeType.Road;
        
        // add the neighbours for the connection
        startNode.Neighbours.Add(endNode);
        endNode.Neighbours.Add(startNode);
        
        Instantiate(roadStructure, _startingNode.transform.position, Quaternion.identity);
        Instantiate(roadStructure, _currentMousePosition, Quaternion.identity);
        
        RemovePlanning();
    }

    public void ReleasedMouse()
    {
        RemovePlanning();
    }

    private void RemovePlanning()
    {
        RemoveIndicators();
        Destroy(_startingNode?.gameObject);
    }

    private void PlaceStartingNode(Vector3Int position)
    {
        _startingNode = Instantiate(potentialPlacementIndicator, position, Quaternion.identity);
    }

    public void PlaceRoad(Vector3Int position)
    {
        PlaceNode(position, NodeType.Road);
    }

    private void PlaceNode(Vector3Int position, NodeType type)
    {
        var gridPosition = WorldToGrid(position);
        _grid[gridPosition.x, gridPosition.y].Type = type;
        var newStructure = Instantiate(roadStructure, position, Quaternion.identity); // todo change roadStructure to be more generic
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
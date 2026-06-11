using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Action FinishedBuildingRoads { get; set; } 
    
    [SerializeField] private GameObject roadStructure;
    [SerializeField] private GridManager gridManager;
    
    private GameObject _startingNode;
    private Vector3Int _startingPosition;
    private Vector3Int _lastSuccessfulPosition;
    private List<(int x, int y)> _validNeighbourNodes = new();

    public void StartRoadPlacement(Vector3Int position)
    {
        if (!IsPositionInBound(position)) return;
        if (!IsPositionFree(position)) return;
        
        RemoveStartingNode();

        var gridPosition = gridManager.WorldToGrid(position);
        _validNeighbourNodes = gridManager.Grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
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

        var gridTargetPosition = gridManager.WorldToGrid(targetPosition);

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
            gridManager.WorldToGrid(new Vector3Int(_startingPosition.x, _startingPosition.y, _startingPosition.z));

        var startNode = gridManager.Grid[startGridPosition.x, startGridPosition.y];
        var endNode = gridManager.Grid[endGridPosition.x, endGridPosition.y];

        // change to road if not already
        if (startNode.Type is NodeType.Empty) startNode.Type = NodeType.Road;
        if (endNode.Type is NodeType.Empty) endNode.Type = NodeType.Road;
        
        // add the neighbours for the connection
        startNode.Neighbours.Add(endNode);
        endNode.Neighbours.Add(startNode);
        
        gridManager.BuildRoadMesh(startGridPosition.x, startGridPosition.y);
        gridManager.BuildRoadMesh(endGridPosition.x, endGridPosition.y);
        
        // Instantiate(roadStructure, _startingPosition, Quaternion.identity);
        // Instantiate(roadStructure, _lastSuccessfulPosition, Quaternion.identity);
        
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
        var gridPosition = gridManager.WorldToGrid(position);
        return gridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Empty ||
               gridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Road;
    }

    private bool IsPositionInBound(Vector3Int position)
    {
        var gridPosition = gridManager.WorldToGrid(position);
        return gridPosition.x >= 0 && gridPosition.x < gridManager.Width && gridPosition.y >= 0 && gridPosition.y < gridManager.Height;
    }

    public void RemoveStartingNode()
    {
        if (_startingNode is null) return;
        Destroy(_startingNode.gameObject);
        _startingNode = null;
        FinishedBuildingRoads?.Invoke();
    }
}
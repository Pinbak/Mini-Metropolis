using System;
using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
using Roads;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Action FinishedBuildingRoads { get; set; } 
    
    [SerializeField] private GameObject roadStructure;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private IntersectionManager intersectionManager;
    
    private GameObject _startingNode;
    private Vector3Int _startingPosition;
    private Vector3Int _lastSuccessfulPosition;
    private List<(int x, int y)> _validNeighbourNodes = new();
    private BuildingMode _mode;

    public void PlaceBuilding(Building information)
    {
        for (var x = 0; x < information.Width; x++)
        for (var y = 0; y < information.Height; y++)
        {
            var gridPosition = new Vector2Int(information.BottomLeft.X + x, information.BottomLeft.Y + y);
            var node = gridManager.Grid[gridPosition.x, gridPosition.y];
            if (node.Type is not NodeType.Empty) return;
            node.Type = information.Layout[x, y];
        }
    }

    public void StartRoadPlacement(Vector3Int position)
    {
        if (_mode == BuildingMode.Bulldozing) return;
        if (!IsPositionInBound(position)) return;
        if (!IsPositionFree(position)) return;
        
        RemoveStartingNode();

        var gridPosition = gridManager.WorldToGrid(position);
        _validNeighbourNodes = gridManager.Grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
        RemoveIllegalPlacements(position); // removes the ability to cross an existing road
        PlaceStartingNode(position);
    }

    public void MouseDown(Vector3 position)
    {
        if (_mode == BuildingMode.Bulldozing) RemoveNode(position);
        if (_mode == BuildingMode.Road) CheckPlacingRoad(position);
    }

    private void RemoveNode(Vector3 position)
    {
        var intPosition = new Vector3Int(Mathf.RoundToInt(position.x), 0, Mathf.RoundToInt(position.z));
        if (!IsPositionInBound(intPosition)) return;
        var gridPosition = gridManager.WorldToGrid(intPosition);
        var nodeToRemove = gridManager.Grid[gridPosition.x, gridPosition.y];
        if (nodeToRemove.Type is not NodeType.Road) return;

        var toRemove = new List<Node>{nodeToRemove};
        var toUpdate = new List<(int, int)> {(nodeToRemove.X, nodeToRemove.Y)};
        foreach (var neighbour in nodeToRemove.Neighbours)
        {
            // have to delete dependent nodes
            if (neighbour.Neighbours.Count == 1)
            {
                toRemove.Add(neighbour);
                toUpdate.Add((neighbour.X, neighbour.Y));
            }
        }

        foreach (var node in toRemove)
        {
            foreach (var neighbour in node.Neighbours)
            {
                neighbour.Neighbours.Remove(node);
                toUpdate.Add((neighbour.X, neighbour.Y));
                
                if (neighbour.Neighbours.Count < 3)
                    intersectionManager.RemoveIntersection(neighbour.X, neighbour.Y);
            }

            node.Neighbours = new List<Node>();
            node.Type = NodeType.Empty;
            intersectionManager.RemoveIntersection(node.X, node.Y);
        }

        var chunksToRefresh = gridManager.GetUniqueChunksFromPositions(toUpdate);
        foreach (var (chunkX, chunkY) in chunksToRefresh)
        {
            gridManager.BuildChunk(chunkX, chunkY);
        }
        
    }

    private void CheckPlacingRoad(Vector3 position)
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

        if (startNode.Neighbours.Count > 2)
            intersectionManager.CreateIntersection(startNode);
        if (endNode.Neighbours.Count > 2)
            intersectionManager.CreateIntersection(endNode);
        
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

    private void RemoveIllegalPlacements(Vector3Int position)
    {
        var gridPosition = gridManager.WorldToGrid(position); // todo make into field
        var diagonals = gridManager.Grid.GetDiagonalCells(gridPosition.x, gridPosition.y);
        var illegalPlacements = new List<(int x, int y)>();

        foreach (var (dX, dY) in diagonals)
        {
            var sharedNeighbours = gridManager.Grid.GetSharedNeighbours(gridPosition.x, gridPosition.y, dX, dY);
            // check if any of the shared neighbours are connected
            foreach (var sharedNeighbour in sharedNeighbours)
            foreach (var sharedNeighbour2 in sharedNeighbours)
            {
                if (sharedNeighbour == sharedNeighbour2) continue;
                var sharedNeighbourNode = gridManager.Grid[sharedNeighbour.x, sharedNeighbour.y];
                var sharedNeighbour2Node = gridManager.Grid[sharedNeighbour2.x, sharedNeighbour2.y];
                if (sharedNeighbourNode.Neighbours.Contains(sharedNeighbour2Node))
                    illegalPlacements.Add((dX, dY));
            }
        }

        foreach (var illegalPlacement in illegalPlacements)
        {
            _validNeighbourNodes.Remove(illegalPlacement);
        }
        
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
        if (_mode == BuildingMode.Bulldozing) return;
        if (_startingNode is null) return;
        Destroy(_startingNode.gameObject);
        _startingNode = null;
        FinishedBuildingRoads?.Invoke();
    }

    public void ChangeMode()
    {
        if (_mode == BuildingMode.Road)
            _mode = BuildingMode.Bulldozing;
        else if (_mode == BuildingMode.Bulldozing)
            _mode = BuildingMode.Road;
        Debug.Log($"Changed mode to {_mode}");
    }
}
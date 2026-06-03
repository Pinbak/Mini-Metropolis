using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int _offsetX;
    private int _offsetY;
    [SerializeField] private GameObject roadStructure;
    private Grid _grid;
    private GameObject _startingNode;
    private GameObject _endingNode;
    
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

    public void StartRoadPlacement(Vector3Int position)
    {
        if (!IsPositionInBound(position)) return;
        if (!IsPositionFree(position)) return;
        PlaceStartingNode(position);
    }

    public void MouseDown(Vector3Int position)
    {
        // throw new NotImplementedException();
    }

    public void EndRoadPlacement()
    {
        Destroy(_startingNode?.gameObject);
        Destroy(_endingNode?.gameObject);
    }

    private void PlaceStartingNode(Vector3Int position)
    {
        _startingNode = Instantiate(roadStructure, position, Quaternion.identity);
    }
    
    public void PlaceRoad(Vector3Int position)
    {
        PlaceNode(position, NodeType.Road);
    }

    private void PlaceNode(Vector3Int position, NodeType type)
    {
        var gridPosition = WorldToGrid(position);
        _grid[gridPosition.x, gridPosition.y] = type;
        var newStructure = Instantiate(roadStructure, position, Quaternion.identity); // todo change roadStructure to be more generic
    }

    private bool IsPositionFree(Vector3Int position)
    {
        var gridPosition = WorldToGrid(position);
        return _grid[gridPosition.x, gridPosition.y] == NodeType.Empty;
    }

    private bool IsPositionInBound(Vector3Int position)
    {
        var gridPosition = WorldToGrid(position);
        return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
    }
}
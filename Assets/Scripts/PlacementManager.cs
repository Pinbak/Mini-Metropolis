using System;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int _offsetX;
    private int _offsetY;
    [SerializeField] private GameObject roadStructure;
    private Grid _grid;
    
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

    public void PlaceRoad(Vector3Int position)
    {
        if (!IsPositionInBound(position)) return;
        if (!IsPositionFree(position)) return;
        PlaceNode(position, NodeType.Road);
    }

    private void PlaceNode(Vector3Int position, NodeType type)
    {
        var gridPosition = WorldToGrid(position);
        _grid[gridPosition.x, gridPosition.y] = type;
        var newStructure = Instantiate(roadStructure, position, Quaternion.identity);
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
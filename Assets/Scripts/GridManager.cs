using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int _offsetX;
    private int _offsetY;

    public Grid Grid { get; private set; }
    public int Width => width;
    public int Height => height;

    private void Start()
    {
        Grid = new Grid(width, height);
        _offsetX = width / 2;
        _offsetY = height / 2;
    }
    
    public Vector2Int WorldToGrid(Vector3Int worldPosition)
    {
        return new Vector2Int(
            worldPosition.x + _offsetX,
            worldPosition.z + _offsetY
        );
    }
    
    public Vector3Int GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3Int(
            gridPosition.x - _offsetX,
            0,
            gridPosition.y - _offsetY);
    }

    public bool GridExists() => Grid is not null;
}
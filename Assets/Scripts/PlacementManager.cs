using Intersections;
using Needs.Buildings;
using Placement;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public LineRenderer PlacementIndicator { get; set; }
    
    // states
    private IPlacementState _mode;
    private Bulldozing _bulldozingState;
    private BuildingRoad _buildingRoadState;

    private void Start()
    {
        _buildingRoadState = new BuildingRoad(this);
        _bulldozingState = new Bulldozing(this);
        _mode = _buildingRoadState;
    }
    
    public void HandleKeyboardPress(KeyboardKeys key)
    {
        _mode.KeyboardPress(key);
    }

    public void HandleMouseHeldDown(Vector3 position)
    {
        _mode.MouseDown(position);
    }

    public void HandleMouseClick(Vector3 position)
    {
        _mode.MouseClick(position);
    }

    public void HandleMouseRelease()
    {
        _mode.MouseRelease();
    }

    public void ChangeMode()
    {
        if (_mode is BuildingRoad)
            _mode = _bulldozingState;
        else if (_mode is Bulldozing)
            _mode = _buildingRoadState;
        Debug.Log($"Changed mode to {_mode.GetType()}");
    }

    public bool IsSpaceAvailable(int width, int height, Node bottomLeft)
    {
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var gridPosition = new Vector2Int(bottomLeft.X + x, bottomLeft.Y + y);
            var node = GridManager.Grid[gridPosition.x, gridPosition.y];
            if (node.Type is not NodeType.Empty) return false;
        }

        return true;
    }

    /// <summary>
    ///     Warning is destructive!
    /// </summary>
    public void PlaceBuilding(Building building)
    {
        for (var x = 0; x < building.Width; x++)
        for (var y = 0; y < building.Height; y++)
        {
            var gridPosition = new Vector2Int(building.BottomLeft.X + x, building.BottomLeft.Y + y);
            var node = GridManager.Grid[gridPosition.x, gridPosition.y];
            node.Type = building.Layout[x, y];
        }
    }
    
    public void PlaceZone(Zone zone)
    {
        for (var x = 0; x < zone.Width; x++)
        for (var y = 0; y < zone.Height; y++)
        {
            var gridPosition = new Vector2Int(zone.BottomLeft.X + x, zone.BottomLeft.Y + y);
            var node = GridManager.Grid[gridPosition.x, gridPosition.y];
            if (node.Type is not NodeType.Empty) return;
            node.Type = zone.Layout[x, y];
        }
    }

    public bool IsPositionFreeOrRoad(Vector3Int position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        return GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Empty ||
               GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Road;
    }

    public bool IsPositionInBound(Vector3Int position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        return gridPosition.x >= 0 && gridPosition.x < GridManager.Width && gridPosition.y >= 0 && gridPosition.y < GridManager.Height;
    }
}
using Buildings;
using Intersections;
using Placement;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public LineRenderer PlacementIndicator { get; set; }
    [field:SerializeField] public BuildingManager BuildingManager { get; set; }
    
    [field:SerializeField] public Zone ZonePrefab { get; set; }
    [field:SerializeField] public Building ResidentialLowWealthPrefab { get; set; }
    [field:SerializeField] public Building CommercialLowWealthPrefab { get; set; }
    [field:SerializeField] public Building IndustrialLowWealthPrefab { get; set; }

    [field:SerializeField] public ColourSampler ColourSampler { get; set; }
    
    // states
    private IPlacementState _mode;
    private Bulldozing _bulldozingState;
    private BuildingRoad _buildingRoadState;
    private BuildingZone _buildingZoneState;

    private void Start()
    {
        _buildingRoadState = new BuildingRoad(this);
        _bulldozingState = new Bulldozing(this);
        _buildingZoneState = new BuildingZone(this);
        _mode = _buildingRoadState;
    }
    
    public void MouseMove(Vector3 position)
    {
        _mode.MouseMove(position);
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
        // todo is temporary
        if (_mode is BuildingRoad)
            _mode = _bulldozingState;
        else if (_mode is Bulldozing)
        {
            _buildingZoneState.ChangePlacementBuilding(ResidentialLowWealthPrefab);
            _mode = _buildingZoneState;
        }
        else if (_mode is BuildingZone && _buildingZoneState.Places.Type is BuildingType.Residential)
        {
            _buildingZoneState.ChangePlacementBuilding(IndustrialLowWealthPrefab);
            _mode = _buildingZoneState;
        }
        else if (_mode is BuildingZone && _buildingZoneState.Places.Type is BuildingType.Industrial)
        {
            _buildingZoneState.ChangePlacementBuilding(CommercialLowWealthPrefab);
            _mode = _buildingZoneState;
        }
        else if (_mode is BuildingZone && _buildingZoneState.Places.Type is BuildingType.Commercial)
        {
            _mode = _buildingRoadState;
            PlacementIndicator.enabled = false;
        }
        Debug.Log($"Changed mode to {_mode.GetType()}");
    }

    public bool IsPositionFreeOrRoad(Vector3Int position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        return GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Empty ||
               GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Road;
    }

    public bool IsPositionInBound(Vector3 position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        return gridPosition.x >= 0 && gridPosition.x < GridManager.Width && gridPosition.y >= 0 && gridPosition.y < GridManager.Height;
    }
    
    public bool IsPositionInBound(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < GridManager.Width && gridPosition.y >= 0 && gridPosition.y < GridManager.Height;
    }
}
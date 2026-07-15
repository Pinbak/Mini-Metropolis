using Buildings;
using Intersections;
using Placement;
using Roads;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public BuildingManager BuildingManager { get; set; }
    
    [field:SerializeField] public Zone ZonePrefab { get; set; }
    [field:SerializeField] public Building ResidentialLowWealthPrefab { get; set; }
    [field:SerializeField] public Building CommercialLowWealthPrefab { get; set; }
    [field:SerializeField] public Building IndustrialLowWealthPrefab { get; set; }
    [field:SerializeField] public Building SchoolPrefab { get; set; }

    [field:SerializeField] public ColourSampler ColourSampler { get; set; }
    [field:SerializeField] public PlacementIndicator PlacementIndicator { get; set; }
    
    // states
    private IPlacementState Mode
    {
        get => _mode;
        set
        {
            _mode?.ExitState();
            _mode = value;
            _mode.EnterState();
        }
    }

    private IPlacementState _mode;
    private Bulldozing _bulldozingState;
    private BuildingRoad _buildingRoadState;
    public BuildingZone BuildingZoneState { get; private set; }

    private void Start()
    {
        _buildingRoadState = new BuildingRoad(this);
        _bulldozingState = new Bulldozing(this);
        BuildingZoneState = new BuildingZone(this);
        Mode = _buildingRoadState;
    }
    
    public void MouseMove(Vector3 position)
    {
        Mode.MouseMove(position);
    }
    
    public void HandleKeyboardPress(KeyboardKeys key)
    {
        if (key is KeyboardKeys.B) ChangeMode();
        else
            Mode.KeyboardPress(key);
    }

    public void HandleMouseHeldDown(Vector3 position)
    {
        Mode.MouseDown(position);
    }

    public void HandleMouseClick(Vector3 position)
    {
        Mode.MouseClick(position);
    }

    public void HandleMouseRelease()
    {
        Mode.MouseRelease();
    }

    private void ChangeMode()
    {
        // todo is temporary
        if (Mode is BuildingRoad)
            Mode = _bulldozingState;
        else if (Mode is Bulldozing)
        {
            BuildingZoneState.ChangePlacementBuilding(ResidentialLowWealthPrefab);
            Mode = BuildingZoneState;
        }
        else if (Mode is BuildingZone && BuildingZoneState.Places.Type is BuildingType.Residential)
        {
            BuildingZoneState.ChangePlacementBuilding(IndustrialLowWealthPrefab);
            Mode = BuildingZoneState;
        }
        else if (Mode is BuildingZone && BuildingZoneState.Places.Type is BuildingType.Industrial)
        {
            BuildingZoneState.ChangePlacementBuilding(CommercialLowWealthPrefab);
            Mode = BuildingZoneState;
        }
        else if (Mode is BuildingZone && BuildingZoneState.Places.Type is BuildingType.Commercial)
        {
            BuildingZoneState.ChangePlacementBuilding(SchoolPrefab);
            Mode = BuildingZoneState;
        }
        else if (Mode is BuildingZone && BuildingZoneState.Places.Type is BuildingType.School)
        {
            Mode = _buildingRoadState;
        }
        Debug.Log($"Changed mode to {Mode.GetType()}");
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
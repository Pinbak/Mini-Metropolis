using Buildings;
using Intersections;
using Placement;
using UnityEngine;

/// <summary>
///     The class for placing buildings and road <see cref="Mode"/> is the current mode.
/// </summary>
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
    [field:SerializeField] public Building FireStationPrefab { get; set; }
    [field:SerializeField] public Building PoliceStationPrefab { get; set; }
    [field:SerializeField] public Building ParkPrefab { get; set; }

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
    private BuildingRoad _buildingRoadState;
    public Bulldozing BulldozingState { get; private set; }
    public BuildingZone BuildingZoneState { get; private set; }
    private NoBuilding _noBuilding;

    private void Start()
    {
        _buildingRoadState = new BuildingRoad(this);
        BulldozingState = new Bulldozing(this);
        BuildingZoneState = new BuildingZone(this);
        _noBuilding = new NoBuilding();
        Mode = _noBuilding;
    }
    
    public void MouseMove(Vector3 position)
    {
        Mode.MouseMove(position);
    }
    
    public void HandleKeyboardPress(KeyboardKeys key)
    {
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

    public void ChangeModeToBulldozing() => ChangeMode(BulldozingState);
    public void ChangeModeToBuildingRoad() => ChangeMode(_buildingRoadState);
    public void ChangeModeToResidentialZone() => ChangeModeToBuildingZone(ResidentialLowWealthPrefab);
    public void ChangeModeToCommercialZone() => ChangeModeToBuildingZone(CommercialLowWealthPrefab);
    public void ChangeModeToIndustrialZone() => ChangeModeToBuildingZone(IndustrialLowWealthPrefab);
    public void ChangeModeToSchool() => ChangeModeToBuildingZone(SchoolPrefab);
    public void ChangeModeToPoliceStation() => ChangeModeToBuildingZone(PoliceStationPrefab);
    public void ChangeModeToFireStation() => ChangeModeToBuildingZone(FireStationPrefab);
    public void ChangeModeToPark() => ChangeModeToBuildingZone(ParkPrefab);

    private void ChangeModeToBuildingZone(Building building)
    {
        BuildingZoneState.ChangePlacementBuilding(building);
        ChangeMode(BuildingZoneState, building);
    }

    private void ChangeMode(IPlacementState mode, Building building = null)
    {
        if (Mode == mode && BuildingZoneState.Places != building)
            Mode = _noBuilding;
        else
            Mode = mode;
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
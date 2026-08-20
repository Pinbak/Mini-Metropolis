using Buildings;
using Junctions;
using Placement;
using UnityEngine;

/// <summary>
///     The class for placing buildings and road. <see cref="Mode"/> is the current mode.
/// </summary>
public class PlacementManager : MonoBehaviour
{
    // All the objects needed, a lot are needed for the mode
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public JunctionManager JunctionManager { get; set; }
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
    
    // States
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
        // create all the states, so that they exist when changed to
        _buildingRoadState = new BuildingRoad(this);
        BulldozingState = new Bulldozing(this);
        BuildingZoneState = new BuildingZone(this);
        _noBuilding = new NoBuilding();
        // select current state as nothing in particular
        Mode = _noBuilding;
    }
    
    // Invoke the mode methods for the current mode when certain keys are pressed
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
    
    public void HandleRightMouseClick()
    {
        Mode = _noBuilding;
    }

    // The methods that are called when clicking the UI buttons
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
        ChangeMode(BuildingZoneState);
    }

    private void ChangeMode(IPlacementState mode) => Mode = mode;
    
    /// <summary>
    ///     Given a position, check if it is free or a road. It's necessary to check if a road is there, as roads can
    ///     connect to existing roads.
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns>Whether the position is either free or a road.</returns>
    public bool IsPositionFreeOrRoad(Vector3Int position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        return GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Empty ||
               GridManager.Grid[gridPosition.x, gridPosition.y].Type == NodeType.Road;
    }

    /// <summary>
    ///     Checks if the given position is within the <see cref="Grid"/>.
    /// </summary>
    public bool IsPositionInBound(Vector3 position)
    {
        var gridPosition = GridManager.WorldToGrid(position);
        // bound check
        return gridPosition.x >= 0 && gridPosition.x < GridManager.Width && gridPosition.y >= 0 && gridPosition.y < GridManager.Height;
    }
    
    /// <summary>
    ///     Checks if the given grid position is within the <see cref="Grid"/>.
    /// </summary>
    public bool IsPositionInBound(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < GridManager.Width && gridPosition.y >= 0 && gridPosition.y < GridManager.Height;
    }
}
using System.Collections.Generic;
using System.Linq;
using Agents;
using Buildings;
using Intersections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     This class is used for managing all the building specific methods and properties. Such as, placing buildings, simulating
///     RCI, supplies, demands, income and balance.
/// </summary>
public class BuildingManager : MonoBehaviour
{
    // how much money the player has
    [SerializeField] private int balance;

    public int Balance
    {
        get => balance;
        set
        {
            balance = value;
            // update the UI to reflect the change
            BalanceText.text = $"£{value}";
        }
    }
    
    [field:SerializeField] public float ResidentialDemand { get; set; }
    [field:SerializeField] public float BaseResidentialDemand { get; set; }
    [field:SerializeField] public float CommercialDemand { get; set; }
    [field:SerializeField] public float BaseCommercialDemand { get; set; }
    [field:SerializeField] public float IndustrialDemand { get; set; }
    [field:SerializeField] public float BaseIndustrialDemand { get; set; }
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on
    [field: SerializeField] public TextMeshProUGUI BalanceText { get; set; } // the ui text showing the player's money

    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; } // this is so it can be set in the inspector, passed down to the cars
    [SerializeField] private PlacementManager placementManager;

    // the UI for the RCI
    [SerializeField] private Slider residentialVisual;
    [SerializeField] private Slider commercialVisual;
    [SerializeField] private Slider industrialVisual;

    public List<Zone> ResidentialZones { get; } = new(); // all the current residential zones
    public List<Zone> CommercialZones { get; } = new(); // all the current commercial zones
    public List<Zone> IndustrialZones { get; }= new(); // all the current industrial zones
    public Building[,] AllBuildings { get; private set; } // to keep track of all the buildings currently built
    public Zone[,] AllZones { get; private set; } // to keep track of all the zones currently placed
    
    // these 4 are used for the supply and demand calculations
    private readonly Dictionary<AgentType, Queue<Building>> _demands = new();
    private readonly Dictionary<AgentType, Queue<Building>> _supplies = new();
    // this is needed to identify which agent is produced by which building, which is used for simulating needs
    private readonly Dictionary<AgentType, BuildingType> _rciSupplies = new()
    {
        {AgentType.Commuter, BuildingType.Residential},
        {AgentType.Shopper, BuildingType.Residential},
        {AgentType.Student, BuildingType.Residential},
        {AgentType.Police, BuildingType.PoliceStation},
        {AgentType.Fire, BuildingType.FireStation},
    };
    // this is needed to identify which agent is demanded by which building, which is used for simulating needs
    private readonly Dictionary<AgentType, BuildingType> _rciSDemands = new()
    {
        {AgentType.Commuter, BuildingType.Industrial},
        {AgentType.Shopper, BuildingType.Commercial},
        {AgentType.Student, BuildingType.School},
        {AgentType.Police, BuildingType.Commercial},
        {AgentType.Fire, BuildingType.Industrial},
    };
    
    private void Start()
    {
        // initialise the arrays
        AllBuildings = new Building[GridManager.Width, GridManager.Height];
        AllZones = new Zone[GridManager.Width, GridManager.Height];
        BalanceText.text = $"£{balance}";
    }

    private void Update()
    {
        // this loop is used to match the supplying buildings to the demanding buildings, and is what gets the agents
        // to move in the first place
        foreach (var (type, buildings) in _supplies)
        {
            // if there are none, return
            if (buildings.Count == 0) continue;
            // if a corresponding demand that satisfies this need doesn't exist, return
            if (!_demands.TryGetValue(type, out var demand)) continue;
            if (demand.Count == 0) continue;
            // get the next building that's supplying and demanding
            var supplyBuilding = _supplies[type].Dequeue();
            // if the buildings are already being used, they shouldn't be in the queue in the first place
            if (supplyBuilding.ToRemove) return;
            var demandBuilding = _demands[type].Dequeue();
            if (demandBuilding.ToRemove) return;
            
            // ask the supplying building to send an agent to the demanding building 
            supplyBuilding.GoTo(demandBuilding, type);
        }
        
        // Once a zone has been placed of any type, the base demand increases slowly to ensure zones are grown sequentially,
        // rather than all at once
        if (ResidentialZones.Count > 0 && BaseResidentialDemand < 2f)
            BaseResidentialDemand += Time.deltaTime;
        if (CommercialZones.Count > 0 && BaseCommercialDemand < 2f)
            BaseCommercialDemand += Time.deltaTime;
        if (IndustrialZones.Count > 0 && BaseIndustrialDemand < 2f)
            BaseIndustrialDemand += Time.deltaTime;
        
        
        // these 3 foreach loops build the growable buildings is the demand is met
        foreach (var residentialZone in ResidentialZones.ToList())
        {
            if (ResidentialDemand >= 0f && BaseResidentialDemand > 2f)
            {
                BaseResidentialDemand -= 2f; // makes sure to remove the base demand, preventing another building from immediately growing
                BuildFromZone(residentialZone);
            }
        }
        
        foreach (var commercialZone in CommercialZones.ToList())
        {
            if (CommercialDemand >= 0f && BaseCommercialDemand > 2f)
            {
                BaseCommercialDemand -= 2f;
                BuildFromZone(commercialZone);
            }
        }
        
        foreach (var industrialZone in IndustrialZones.ToList())
        {
            if (IndustrialDemand >= 0f && BaseIndustrialDemand > 2f)
            {
                BaseIndustrialDemand -= 2f;
                BuildFromZone(industrialZone);
            }
        }
        
    }

    private void UpdateRci()
    {
        // restart the values
        ResidentialDemand = 0f;
        CommercialDemand = 0f;
        IndustrialDemand = 0f;
        
        // goes through all the demanding buildings in the queue, adding the corresponding need to the demands
        // for example, a commercial building that is low in shoppers, needs to demand residential buildings, not more
        // commercial buildings, as the residential buildings fulfill the demand
        foreach (var (type, buildings) in _demands)
        {
            if (buildings.Count == 0) continue;
            // try to get the corresponding building, for example, residential for commercial, as it fulfills its needs
            if (!_rciSupplies.TryGetValue(type, out var supplyZone)) continue;
            switch (supplyZone)
            {
                case BuildingType.Residential:
                    ResidentialDemand += buildings.Count;
                    break;
                case BuildingType.Commercial:
                    CommercialDemand += buildings.Count;
                    break;
                case BuildingType.Industrial:
                    IndustrialDemand += buildings.Count;
                    break;
            }
            // same for any demands, here instead, the corresponding RCI is reduced instead
            if (!_rciSDemands.TryGetValue(type, out var demandZone)) continue;
            switch (demandZone)
            {
                case BuildingType.Residential:
                    ResidentialDemand -= buildings.Count;
                    break;
                case BuildingType.Commercial:
                    CommercialDemand -= buildings.Count;
                    break;
                case BuildingType.Industrial:
                    IndustrialDemand -= buildings.Count;
                    break;
            }
        }
        // repeats the process from above, but for the supplying building queue
        foreach (var (type, buildings) in _supplies)
        {
            if (buildings.Count == 0) continue;
            if (!_rciSDemands.TryGetValue(type, out var demandZone)) continue;
            switch (demandZone)
            {
                case BuildingType.Residential:
                    ResidentialDemand += buildings.Count;
                    break;
                case BuildingType.Commercial:
                    CommercialDemand += buildings.Count;
                    break;
                case BuildingType.Industrial:
                    IndustrialDemand += buildings.Count;
                    break;
            }
            if (!_rciSupplies.TryGetValue(type, out var supplyZone)) continue;
            switch (supplyZone)
            {
                case BuildingType.Residential:
                    ResidentialDemand -= buildings.Count;
                    break;
                case BuildingType.Commercial:
                    CommercialDemand -= buildings.Count;
                    break;
                case BuildingType.Industrial:
                    IndustrialDemand -= buildings.Count;
                    break;
            }
        }

        // sets the UI RCI values to the new RCI values
        residentialVisual.value = Mathf.Clamp(ResidentialDemand, 0, 5);
        commercialVisual.value = Mathf.Clamp(CommercialDemand, 0, 5);
        industrialVisual.value = Mathf.Clamp(IndustrialDemand, 0, 5);
    }

    /// <summary>
    ///     Adds the building to the supply queue, used when the building is low on a need
    /// </summary>
    public void AddToSupplyQueue(Building building, Need need)
    {
        // initialise the dictionary key if not already
        if (!_supplies.ContainsKey(need.Type))
            _supplies[need.Type] = new Queue<Building>();
        // add the building to the queue and then recalculate the RCI
        _supplies[need.Type].Enqueue(building);
        UpdateRci();
    }
    
    /// <summary>
    ///     Adds the building to the demand queue, used when the building is low on a need
    /// </summary>
    public void AddToDemandQueue(Building building, Need need)
    {
        // initialise the dictionary key if not already
        if (!_demands.ContainsKey(need.Type))
            _demands[need.Type] = new Queue<Building>();
        // add the building to the queue and then recalculate the RCI
        _demands[need.Type].Enqueue(building);
        UpdateRci();
    }
    
    /// <summary>
    ///     The actual method which takes a growable zone and turns it into a building, removing the old gameobject,
    ///     replacing it with a new one.
    /// </summary>
    private void BuildFromZone(Zone zone)
    {
        // get the prefab to build, which is stored by the zone
        var buildingPrefab = zone.Builds;
        // create the new building from this prefab
        var building = Instantiate(buildingPrefab, zone.transform.position, zone.transform.rotation, transform);
        building.Init(this); // no constructor, so Init is used in the same context
        // updates the grid to reflect the changes
        placementManager.BuildingZoneState.PlaceBuilding(building);
        // destroy the zone now, as all information has been gathered from it
        Destroy(zone.gameObject);
        // manage the zones list
        if (zone.Builds.Type is BuildingType.Residential)
            ResidentialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Commercial)
            CommercialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Industrial)
            IndustrialZones.Remove(zone);
    }

    /// <summary>
    ///     Change a building, used for upgrading and downgrading the given building
    /// </summary>
    public void ChangeBuilding(Building currentBuilding, Building buildingToChangeTo)
    {
        // create a new building
        var building = Instantiate(buildingToChangeTo, currentBuilding.transform.position,
            currentBuilding.transform.rotation, transform);
        // remove the old one
        placementManager.BulldozingState.RemoveBuilding(currentBuilding);
        building.Init(this); // init the new one here, as no constructor
        // update the grid to reflect changes
        placementManager.BuildingZoneState.PlaceBuilding(building);
    }
}
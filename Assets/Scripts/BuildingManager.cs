using System.Collections.Generic;
using System.Linq;
using Intersections;
using Needs.Agents;
using Needs.Buildings;
using Placement;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [field:SerializeField] public float ResidentialDemand { get; set; }
    [field:SerializeField] public float CommercialDemand { get; set; }
    [field:SerializeField] public float IndustrialDemand { get; set; }
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on

    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;

    public List<Zone> ResidentialZones { get; } = new();
    public List<Zone> CommercialZones { get; } = new();
    public List<Zone> IndustrialZones { get; }= new();
    private readonly List<Building> _buildingsWithAvailableParking = new();
    public Building[,] AllBuildings { get; set; }
    private Dictionary<AgentType, Queue<Building>> _demands = new();
    private Dictionary<AgentType, Queue<Building>> _supplies = new();
    
    private void Start()
    {
        AllBuildings = new Building[GridManager.Width, GridManager.Height];
    }

    private void Update()
    {
        foreach (var (type, buildings) in _supplies)
        {
            if (buildings.Count == 0) continue;
            if (!_demands.TryGetValue(type, out var demand)) continue;
            if (demand.Count == 0) continue;
            var supplyBuilding = _supplies[type].Dequeue();
            var demandBuilding = _demands[type].Dequeue();
            supplyBuilding.GoTo(demandBuilding, type);
        }

        foreach (var residentialZone in ResidentialZones.ToList()) BuildFromZone(residentialZone);
        foreach (var industrialZone in IndustrialZones.ToList()) BuildFromZone(industrialZone);
    }

    public void AddToSupplyQueue(Building building, Need need)
    {
        if (!_supplies.ContainsKey(need.Type))
            _supplies[need.Type] = new Queue<Building>();
        _supplies[need.Type].Enqueue(building);
    }
    
    public void AddToDemandQueue(Building building, Need need)
    {
        if (!_demands.ContainsKey(need.Type))
            _demands[need.Type] = new Queue<Building>();
        _demands[need.Type].Enqueue(building);
    }
    
    private void BuildFromZone(Zone zone)
    {
        var buildingPrefab = zone.Builds;
        var building = Instantiate(buildingPrefab, zone.transform.position, Quaternion.identity, transform);
        building.Init(this);
        BuildingZone.PlaceBuilding(building, placementManager);
        AllBuildings[zone.BottomLeft.X, zone.BottomLeft.Y] = building;
        Destroy(zone.gameObject);
        if (zone.Builds.Type is BuildingType.Residential)
            ResidentialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Commercial)
            CommercialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Industrial)
            IndustrialZones.Remove(zone);
    }
    
}
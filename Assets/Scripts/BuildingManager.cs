using System;
using System.Collections.Generic;
using Intersections;
using Needs.Agents;
using Needs.Buildings;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [field:SerializeField] public float ResidentialDemand { get; set; }
    [field:SerializeField] public float CommercialDemand { get; set; }
    [field:SerializeField] public float IndustrialDemand { get; set; }
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on

    [SerializeField] private Zone zonePrefab;
    [SerializeField] private ResidentialBuilding residentialLowWealthPrefab;
    [SerializeField] private ResidentialBuilding residentialHighWealthPrefab;
    [SerializeField] private IndustrialBuilding industrialLowWealthPrefab;

    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;

    private readonly List<Zone> _residentialZones = new();
    private readonly List<Zone> _commercialZones = new();
    private readonly List<Zone> _industrialZones = new();
    private readonly List<Building> _buildingsWithAvailableParking = new();
    private Building[,] _allBuildings;
    private Dictionary<AgentType, Queue<Building>> _demands = new();
    private Dictionary<AgentType, Queue<Building>> _supplies = new();

    public void CreateResidentialZone(Node position)
    {
        CreateZone(position, residentialLowWealthPrefab);
    }
    
    public void CreateCommercialZone(Node position)
    {
        // CreateZone(position, );
    }

    public void CreateIndustrialZone(Node position)
    {
        CreateZone(position, industrialLowWealthPrefab);
    }

    private void CreateZone(Node position, Building type)
    {
        var width = type.Width;
        var height = type.Height;
        if (!placementManager.IsSpaceAvailable(width, height, position)) return;
        var newZone = Instantiate(zonePrefab, GridManager.NodeToWorld(position), Quaternion.identity, transform);
        newZone.Init(this, type);
        placementManager.PlaceZone(newZone);

        switch (type)
        {
            // keep track of zones
            case ResidentialBuilding:
                _residentialZones.Add(newZone);
                break;
            case CommercialBuilding:
                _commercialZones.Add(newZone);
                break;
            case IndustrialBuilding:
                _industrialZones.Add(newZone);
                break;
        }
        
    }

    private void BuildFromZone(Zone zone)
    {
        var buildingPrefab = zone.Builds;
        var building = Instantiate(buildingPrefab, zone.transform.position, Quaternion.identity, transform);
        building.Init(this);
        placementManager.PlaceBuilding(building);
        _allBuildings[zone.BottomLeft.X, zone.BottomLeft.Y] = building;
        Destroy(zone.gameObject);
    }
    
    private void Start()
    {
        _allBuildings = new Building[GridManager.Width, GridManager.Height];
        CreateResidentialZone(GridManager.WorldToNode(new Vector3(2f, 0f, 2f)));
        BuildFromZone(_residentialZones[0]);
        CreateIndustrialZone(GridManager.WorldToNode(new Vector3(0f, 0f, 2f)));
        BuildFromZone(_industrialZones[0]);
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
    
}
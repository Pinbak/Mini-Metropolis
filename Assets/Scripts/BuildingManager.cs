using System.Collections.Generic;
using System.Linq;
using Agents;
using Buildings;
using Intersections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private int balance;

    public int Balance
    {
        get => balance;
        set
        {
            balance = value;
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
    [field: SerializeField] public TextMeshProUGUI BalanceText { get; set; }

    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;

    [SerializeField] private Slider residentialVisual;
    [SerializeField] private Slider commercialVisual;
    [SerializeField] private Slider industrialVisual;

    public List<Zone> ResidentialZones { get; } = new();
    public List<Zone> CommercialZones { get; } = new();
    public List<Zone> IndustrialZones { get; }= new();
    private readonly List<Building> _buildingsWithAvailableParking = new();
    public Building[,] AllBuildings { get; private set; }
    public Zone[,] AllZones { get; private set; }
    private Dictionary<AgentType, Queue<Building>> _demands = new();
    private Dictionary<AgentType, Queue<Building>> _supplies = new();

    private Dictionary<AgentType, BuildingType> _rciSupplies = new()
    {
        {AgentType.Commuter, BuildingType.Residential},
        {AgentType.Shopper, BuildingType.Residential},
        {AgentType.Student, BuildingType.Residential},
        {AgentType.Police, BuildingType.PoliceStation},
        {AgentType.Fire, BuildingType.FireStation},
    };

    private Dictionary<AgentType, BuildingType> _rciSDemands = new()
    {
        {AgentType.Commuter, BuildingType.Industrial},
        {AgentType.Shopper, BuildingType.Commercial},
        {AgentType.Student, BuildingType.School},
        {AgentType.Police, BuildingType.Commercial},
        {AgentType.Fire, BuildingType.Industrial},
    };


    private void Start()
    {
        AllBuildings = new Building[GridManager.Width, GridManager.Height];
        AllZones = new Zone[GridManager.Width, GridManager.Height];
        BalanceText.text = $"£{balance}";
    }

    private void Update()
    {
        foreach (var (type, buildings) in _supplies)
        {
            if (buildings.Count == 0) continue;
            if (!_demands.TryGetValue(type, out var demand)) continue;
            if (demand.Count == 0) continue;
            var supplyBuilding = _supplies[type].Dequeue();
            if (supplyBuilding.ToRemove) return;
            var demandBuilding = _demands[type].Dequeue();
            if (demandBuilding.ToRemove) return;
            supplyBuilding.GoTo(demandBuilding, type);
        }
        
        if (ResidentialZones.Count > 0 && BaseResidentialDemand < 2f)
            BaseResidentialDemand += Time.deltaTime;
        if (CommercialZones.Count > 0 && BaseCommercialDemand < 2f)
            BaseCommercialDemand += Time.deltaTime;
        if (IndustrialZones.Count > 0 && BaseIndustrialDemand < 2f)
            BaseIndustrialDemand += Time.deltaTime;
        
        
        foreach (var residentialZone in ResidentialZones.ToList())
        {
            if (ResidentialDemand >= 0f && BaseResidentialDemand > 2f)
            {
                BaseResidentialDemand -= 2f;
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
        ResidentialDemand = 0f;
        CommercialDemand = 0f;
        IndustrialDemand = 0f;
        foreach (var (type, buildings) in _demands)
        {
            if (buildings.Count == 0) continue;
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

        residentialVisual.value = Mathf.Clamp(ResidentialDemand, 0, 5);
        commercialVisual.value = Mathf.Clamp(CommercialDemand, 0, 5);
        industrialVisual.value = Mathf.Clamp(IndustrialDemand, 0, 5);
    }

    public void AddToSupplyQueue(Building building, Need need)
    {
        if (!_supplies.ContainsKey(need.Type))
            _supplies[need.Type] = new Queue<Building>();
        _supplies[need.Type].Enqueue(building);
        UpdateRci();
    }
    
    public void AddToDemandQueue(Building building, Need need)
    {
        if (!_demands.ContainsKey(need.Type))
            _demands[need.Type] = new Queue<Building>();
        _demands[need.Type].Enqueue(building);
        UpdateRci();
    }
    
    private void BuildFromZone(Zone zone)
    {
        var buildingPrefab = zone.Builds;
        var building = Instantiate(buildingPrefab, zone.transform.position, zone.transform.rotation, transform);
        building.Init(this);
        placementManager.BuildingZoneState.PlaceBuilding(building);
        Destroy(zone.gameObject);
        if (zone.Builds.Type is BuildingType.Residential)
            ResidentialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Commercial)
            CommercialZones.Remove(zone);
        else if (zone.Builds.Type is BuildingType.Industrial)
            IndustrialZones.Remove(zone);
    }

    public void ChangeBuilding(Building currentBuilding, Building buildingToChangeTo)
    {
        var building = Instantiate(buildingToChangeTo, currentBuilding.transform.position,
            currentBuilding.transform.rotation, transform);
        placementManager.BulldozingState.RemoveBuilding(currentBuilding);
        building.Init(this);
        placementManager.BuildingZoneState.PlaceBuilding(building);
    }
}
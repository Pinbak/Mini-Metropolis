using System;
using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [field:SerializeField] public float ResidentialDemand { get; set; }
    [field:SerializeField] public float CommercialDemand { get; set; }
    [field:SerializeField] public float IndustrialDemand { get; set; }
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on

    [SerializeField] private Zone zonePrefab;
    [SerializeField] private Building residentialLowWealthPrefab;
    [SerializeField] private Building residentialHighWealthPrefab;
    [SerializeField] private Building industrialLowWealthPrefab;

    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;

    private readonly List<Zone> _residentialZones = new();
    private readonly List<Zone> _commercialZones = new();
    private readonly List<Zone> _industrialZones = new();
    private readonly List<Building> _buildingsWithAvailableParking = new();
    private Building[,] _allBuildings;

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
        
        // keep track of zones
        switch (type.BuildingType)
        {
            case BuildingType.Residential:
                _residentialZones.Add(newZone);
                break;
            case BuildingType.Commercial:
                _commercialZones.Add(newZone);
                break;
            case BuildingType.Industrial:
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
    }
    
}
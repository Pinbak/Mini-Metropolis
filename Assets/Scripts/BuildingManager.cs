using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on
    [field:SerializeField] public GameObject TestPosition { get; set; }
    
    [SerializeField] private Residential residentialLowWealthPrefab;
    [SerializeField] private Residential residentialHighWealthPrefab;
    [SerializeField] private Industrial industrialLowWealthPrefab;
    
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;
    
    public Industrial TestIndustrial { get; set; } // todo should be a list of available workplaces for example
    public Residential TestHouse { get; set; }
    public Residential TestHouse2 { get; set; }
    
    // private readonly List<Commuter> _testCars = new();
    
    private void Start()
    {
        TestHouse = Instantiate(residentialLowWealthPrefab, new Vector3(0f, 0f, 2f), Quaternion.identity, transform);
        TestHouse.Init(this);
        placementManager.PlaceBuilding(TestHouse.BuildingInformation);
        
        TestHouse2 = Instantiate(residentialLowWealthPrefab, new Vector3(0f, 0f, -2f), Quaternion.identity, transform);
        TestHouse2.Init(this);
        placementManager.PlaceBuilding(TestHouse2.BuildingInformation);

        var vectors = new List<Vector3>()
        {
            new Vector3(-2f, 0f, 2f), new Vector3(-4f, 0f, 2f), new Vector3(-6f, 0f, 2f),
            new Vector3(-2f, 0f, 0f), new Vector3(-4f, 0f, 0f), new Vector3(-6f, 0f, 0f), 
            new Vector3(-2f, 0f, -2f), new Vector3(-4f, 0f, -2f), new Vector3(-6f, 0f, -2f), 
        };

        foreach (var vector3 in vectors)
        {
            var house = Instantiate(residentialLowWealthPrefab, vector3, Quaternion.identity, transform);
            house.Init(this);
            placementManager.PlaceBuilding(house.BuildingInformation);
        }

        TestIndustrial = Instantiate(industrialLowWealthPrefab, new Vector3(2f, 0f, 2f), Quaternion.identity,
            transform);
        TestIndustrial.Init(this);
        placementManager.PlaceBuilding(TestIndustrial.BuildingInformation);
    }
    
}
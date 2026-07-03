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
        
        var testHouse3 = Instantiate(residentialLowWealthPrefab, new Vector3(-2f, 0f, 2f), Quaternion.identity, transform);
        testHouse3.Init(this);
        placementManager.PlaceBuilding(testHouse3.BuildingInformation);
        
        var testHouse4 = Instantiate(residentialLowWealthPrefab, new Vector3(-4f, 0f, 2f), Quaternion.identity, transform);
        testHouse4.Init(this);
        placementManager.PlaceBuilding(testHouse4.BuildingInformation);
        
        var testHouse5 = Instantiate(residentialLowWealthPrefab, new Vector3(-6f, 0f, 2f), Quaternion.identity, transform);
        testHouse5.Init(this);
        placementManager.PlaceBuilding(testHouse5.BuildingInformation);
        
        var testHouse6 = Instantiate(residentialLowWealthPrefab, new Vector3(-2f, 0f, -2f), Quaternion.identity, transform);
        testHouse6.Init(this);
        placementManager.PlaceBuilding(testHouse6.BuildingInformation);
        
        var testHouse7 = Instantiate(residentialLowWealthPrefab, new Vector3(-4f, 0f, -2f), Quaternion.identity, transform);
        testHouse7.Init(this);
        placementManager.PlaceBuilding(testHouse7.BuildingInformation);

        TestIndustrial = Instantiate(industrialLowWealthPrefab, new Vector3(2f, 0f, 2f), Quaternion.identity,
            transform);
        TestIndustrial.Init(this);
        placementManager.PlaceBuilding(TestIndustrial.BuildingInformation);
    }
    
}
using Intersections;
using Needs.Buildings;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on
    [field:SerializeField] public GameObject TestPosition { get; set; }
    
    [SerializeField] private Residential residentialLowWealthPrefab;
    [SerializeField] private Residential residentialHighWealthPrefab;
    
    [field:SerializeField] public GridManager GridManager { get; set; }
    [field:SerializeField] public IntersectionManager IntersectionManager { get; set; }
    [field:SerializeField] public AnimationCurve CarAcceleration { get; set; }
    [SerializeField] private PlacementManager placementManager;
    
    // private readonly List<Commuter> _testCars = new();
    
    private void Start()
    {
        var testHouse = Instantiate(residentialLowWealthPrefab, new Vector3(0f, 0f, 2f), Quaternion.identity, transform);
        testHouse.Init(this);
        placementManager.PlaceBuilding(testHouse.BuildingInformation);
    }
}
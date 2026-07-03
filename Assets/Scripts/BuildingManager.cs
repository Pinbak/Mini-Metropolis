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
    
    // private readonly List<Commuter> _testCars = new();
    
    private void Start()
    {
        var testHouse = Instantiate(residentialLowWealthPrefab, Vector3.zero, Quaternion.identity, transform);
        testHouse.Init(Vector3.zero, this);
    }
}
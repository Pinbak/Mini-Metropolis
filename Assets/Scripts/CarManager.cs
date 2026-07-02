using System.Collections.Generic;
using Intersections;
using Needs;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [field:SerializeField] public LayerMask AgentLayer { get; set; } // the layer the agents are on
    [field:SerializeField] public GameObject TestPosition { get; set; }

    [SerializeField] private Worker workerPrefab;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private IntersectionManager intersectionManager;

    private readonly List<Worker> _testCars = new();
    
    private void Start()
    {
        const int numberOfCars = 20;
        for (var i = 0; i < numberOfCars; i++)
        {
            var testCar = Instantiate(workerPrefab, Vector3.zero, Quaternion.identity, transform);
            testCar.Init(this, gridManager, intersectionManager);
            _testCars.Add(testCar);
        }
    }

    public void CreateTestPath()
    {
        Debug.Log("Attempting to find path");
        foreach (var testCar in _testCars)
        {
            testCar.FindTestPath();
        }
    }
}
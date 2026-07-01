using System.Collections.Generic;
using Needs;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private Worker workerPrefab;
    [SerializeField] private GridManager gridManager;
    [field:SerializeField] public GameObject TestPosition { get; set; }

    private readonly List<Worker> _testCars = new();
    
    private void Start()
    {
        const int numberOfCars = 1;
        for (var i = 0; i < numberOfCars; i++)
        {
            var testCar = Instantiate(workerPrefab, Vector3.zero, Quaternion.identity, transform);
            testCar.Init(this, gridManager);
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
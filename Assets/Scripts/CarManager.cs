using System;
using Needs;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private Worker workerPrefab;
    [SerializeField] private GridManager gridManager;
    [field:SerializeField] public GameObject TestPosition { get; set; }

    private Worker _testCar;
    
    private void Start()
    {
        _testCar = Instantiate(workerPrefab, Vector3.zero, Quaternion.identity, transform);
        _testCar.Init(this, gridManager);
    }

    public void CreateTestPath()
    {
        Debug.Log("Attempting to find path");
        _testCar.FindTestPath();
    }
}
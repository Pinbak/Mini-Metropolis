using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlacementManager placementManager;

    private void Start()
    {
        inputManager.OnMouseDown += HandleMouseClick;
    }

    private void HandleMouseClick(Vector3Int position)
    {
        placementManager.PlaceRoad(position);
    }

    private void Update()
    {
        
    }
}
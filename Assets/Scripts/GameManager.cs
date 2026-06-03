using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlacementManager placementManager;

    private void Start()
    {
        inputManager.OnMouseDown += HandleMouseClick;
        inputManager.OnMouseUp += HandleMouseRelease;
        inputManager.OnMouseHold += HandleMouseHeldDown;
    }

    private void HandleMouseHeldDown(Vector3Int position)
    {
        placementManager.MouseDown(position);
    }

    private void HandleMouseClick(Vector3Int position)
    {
        placementManager.StartRoadPlacement(position);
    }

    private void HandleMouseRelease()
    {
        placementManager.EndRoadPlacement();
    }

    private void Update()
    {
        
    }
}
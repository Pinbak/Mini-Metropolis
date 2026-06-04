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

    private void HandleMouseHeldDown(Vector3 position)
    {
        placementManager.MouseDown(position);
    }

    private void HandleMouseClick(Vector3 position)
    {
        placementManager.StartRoadPlacement(Vector3Int.RoundToInt(position));
    }

    private void HandleMouseRelease()
    {
        // placementManager.ReleasedMouse();
    }

    private void Update()
    {
        
    }
}
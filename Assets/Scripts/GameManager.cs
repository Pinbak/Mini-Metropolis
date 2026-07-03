using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private BuildingManager buildingManager;

    private void Start()
    {
        inputManager.OnMouseDown += HandleMouseClick;
        inputManager.OnMouseUp += HandleMouseRelease;
        inputManager.OnMouseHold += HandleMouseHeldDown;
        inputManager.KeyboardPress += HandleKeyboardPress;
    }

    private void HandleKeyboardPress(KeyboardKeys key)
    {
        placementManager.ChangeMode();
        buildingManager.StartTestMovement();
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
        placementManager.RemoveStartingNode();
    }

    private void Update()
    {
        
    }
}
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
        inputManager.MousePosition += HandleMousePosition;
    }

    private void HandleMousePosition(Vector3 position)
    {
        placementManager.MouseMove(position);
    }

    private void HandleKeyboardPress(KeyboardKeys key)
    {
        placementManager.HandleKeyboardPress(key);
    }

    private void HandleMouseHeldDown(Vector3 position)
    {
        placementManager.HandleMouseHeldDown(position);
    }

    private void HandleMouseClick(Vector3 position)
    {
        placementManager.HandleMouseClick(position);
    }

    private void HandleMouseRelease()
    {
        placementManager.HandleMouseRelease();
    }

}
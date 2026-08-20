using UnityEngine;

/// <summary>
///     A class which links the input manager to the placement and building manager.
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private BuildingManager buildingManager;

    private void Start()
    {
        // subscribes the input manager to all the inputs needed for building
        inputManager.OnMouseDown += HandleMouseClick;
        inputManager.OnRightMouseDown += HandleRightMouseClick;
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

    private void HandleRightMouseClick()
    {
        placementManager.HandleRightMouseClick();
    }

    private void HandleMouseRelease()
    {
        placementManager.HandleMouseRelease();
    }

}
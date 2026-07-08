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
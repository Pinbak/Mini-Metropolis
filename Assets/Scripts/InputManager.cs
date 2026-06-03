using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    public Action<Vector3Int> OnMouseDown { get; set; }
    public Action<Vector3Int> OnMouseHold { get; set; }
    public Action OnMouseUp { get; set; }

    [SerializeField] private InputActionReference leftMouseButton;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ground;
    private Vector2 _cameraMovementVector;

    private void OnEnable()
    {
        leftMouseButton.action.started += IsPointerDown;
        leftMouseButton.action.performed += IsPointerHold;
        leftMouseButton.action.canceled += IsPointerUp;
        leftMouseButton.action.Enable();
    }

    private void OnDisable()
    {
        leftMouseButton.action.started -= IsPointerDown;
        leftMouseButton.action.performed -= IsPointerHold;
        leftMouseButton.action.canceled -= IsPointerUp;
        leftMouseButton.action.Disable();
    }

    private void GetInputs()
    {
        _cameraMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    private void IsPointerHold(InputAction.CallbackContext _)
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        var position = RaycastGround();
        if (position.HasValue)
            OnMouseHold?.Invoke(position.Value);
    }

    private void IsPointerUp(InputAction.CallbackContext _)
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        OnMouseUp?.Invoke();
    }

    private void IsPointerDown(InputAction.CallbackContext _)
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        var position = RaycastGround();
        if (position.HasValue)
            OnMouseDown?.Invoke(position.Value);
    }
    
    /// <summary>
    ///     Get position where the mouse touches the ground layer
    /// </summary>
    private Vector3Int? RaycastGround()
    {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ground))
            return Vector3Int.RoundToInt(hit.point);
        return null;
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    public Action<Vector3> OnMouseDown { get; set; }
    public Action<Vector3> OnMouseHold { get; set; }
    public Action OnMouseUp { get; set; }
    
    public Action<Vector3> MousePosition { get; set; }
    
    public Action<KeyboardKeys> KeyboardPress { get; set; }

    [SerializeField] private InputActionReference leftMouseButton;
    [SerializeField] private InputActionReference keyboardB;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ground;
    private Vector2 _cameraMovementVector;
    private bool _isMouseBeingHeld;

    private void OnEnable()
    {
        leftMouseButton.action.started += IsPointerDown;
        // leftMouseButton.action.performed += IsPointerHold;
        leftMouseButton.action.canceled += IsPointerUp;
        keyboardB.action.performed += _ => KeyboardPress?.Invoke(KeyboardKeys.B); 
        leftMouseButton.action.Enable();
        keyboardB.action.Enable();
    }

    private void OnDisable()
    {
        leftMouseButton.action.started -= IsPointerDown;
        // leftMouseButton.action.performed -= IsPointerHold;
        leftMouseButton.action.canceled -= IsPointerUp;
        leftMouseButton.action.Disable();
        keyboardB.action.Disable();
    }

    private void Update()
    {
        if (_isMouseBeingHeld)
            IsPointerHold();
        
        var groundPosition = RaycastGround();
        if (groundPosition is null) return;
        MousePosition.Invoke((Vector3)groundPosition);
    }

    private void GetInputs()
    {
        _cameraMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    private void IsPointerHold()
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        var position = RaycastGround();
        if (position.HasValue)
            OnMouseHold?.Invoke(position.Value);
    }

    private void IsPointerUp(InputAction.CallbackContext _)
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        _isMouseBeingHeld = false;
        OnMouseUp?.Invoke();
    }

    private void IsPointerDown(InputAction.CallbackContext _)
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        _isMouseBeingHeld = true;
        var position = RaycastGround();
        if (position.HasValue)
            OnMouseDown?.Invoke(position.Value);
    }
    
    /// <summary>
    ///     Get position where the mouse touches the ground layer
    /// </summary>
    private Vector3? RaycastGround()
    {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ground))
            return hit.point;
        return null;
    }
}

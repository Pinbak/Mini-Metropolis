using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
///     The class for collecting and rebroadcasting input.
/// </summary>
public class InputManager : MonoBehaviour
{
    public Action<Vector3> OnMouseDown { get; set; }
    public Action<Vector3> OnMouseHold { get; set; }
    public Action OnMouseUp { get; set; }
    
    public Action<Vector3> MousePosition { get; set; }
    
    public Action<KeyboardKeys> KeyboardPress { get; set; }

    [SerializeField] private InputActionReference leftMouseButton;
    [SerializeField] private InputActionReference keyboardR;
    [SerializeField] private InputActionReference keyboardW;
    [SerializeField] private InputActionReference keyboardA;
    [SerializeField] private InputActionReference keyboardS;
    [SerializeField] private InputActionReference keyboardD;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ground;
    private bool _isMouseBeingHeld;
    private EventSystem _eventSystem;
    private bool _pointerOverUI;

    private void OnEnable()
    {
        leftMouseButton.action.started += IsPointerDown;
        // leftMouseButton.action.performed += IsPointerHold;
        leftMouseButton.action.canceled += IsPointerUp;
        keyboardR.action.performed += _ => KeyboardPress?.Invoke(KeyboardKeys.R);
        leftMouseButton.action.Enable();
        _eventSystem = EventSystem.current;
    }

    private void OnDisable()
    {
        leftMouseButton.action.started -= IsPointerDown;
        // leftMouseButton.action.performed -= IsPointerHold;
        leftMouseButton.action.canceled -= IsPointerUp;
        leftMouseButton.action.Disable();
    }

    private void Update()
    {
        if (keyboardW.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.W);
        if (keyboardA.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.A);
        if (keyboardS.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.S);
        if (keyboardD.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.D);
        
        _pointerOverUI = _eventSystem.IsPointerOverGameObject();
        if (_isMouseBeingHeld)
            IsPointerHold();
        
        var groundPosition = RaycastGround();
        if (groundPosition is null) return;
        MousePosition.Invoke((Vector3)groundPosition);
    }
    
    private void IsPointerHold()
    {
        if (_pointerOverUI) return;
        var position = RaycastGround();
        if (position.HasValue)
            OnMouseHold?.Invoke(position.Value);
    }

    private void IsPointerUp(InputAction.CallbackContext _)
    {
        if (_pointerOverUI) return;
        _isMouseBeingHeld = false;
        OnMouseUp?.Invoke();
    }

    private void IsPointerDown(InputAction.CallbackContext _)
    {
        if (_pointerOverUI) return;
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

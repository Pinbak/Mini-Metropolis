using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
///     The class for collecting and rebroadcasting input.
/// </summary>
public class InputManager : MonoBehaviour
{
    // the rebroadcast events that can be subscribed to by other objects
    public Action<Vector3> OnMouseDown { get; set; }
    public Action OnRightMouseDown { get; set; }
    public Action<Vector3> OnMouseHold { get; set; }
    public Action OnMouseUp { get; set; }
    
    public Action<Vector3> MousePosition { get; set; }
    
    public Action<KeyboardKeys> KeyboardPress { get; set; }

    // the behaviours that are checked
    [SerializeField] private InputActionReference leftMouseButton;
    [SerializeField] private InputActionReference rightMouseButton;
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
        // subscribe to the events from the event manager, invoking certain methods
        leftMouseButton.action.started += IsPointerDown;
        leftMouseButton.action.canceled += IsPointerUp;
        rightMouseButton.action.started += IsRightPointerDown;
        keyboardR.action.performed += _ => KeyboardPress?.Invoke(KeyboardKeys.R);
        leftMouseButton.action.Enable();
        _eventSystem = EventSystem.current;
    }

    private void OnDisable()
    {
        leftMouseButton.action.started -= IsPointerDown;
        leftMouseButton.action.canceled -= IsPointerUp;
        rightMouseButton.action.started -= IsRightPointerDown;
        leftMouseButton.action.Disable();
    }

    private void Update()
    {
        // for these keys, they can be held down, so they are processed here
        if (keyboardW.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.W);
        if (keyboardA.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.A);
        if (keyboardS.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.S);
        if (keyboardD.action.IsPressed()) KeyboardPress?.Invoke(KeyboardKeys.D);
        
        // checks if the cursor is over the UI, if it is, don't create any buildings
        _pointerOverUI = _eventSystem.IsPointerOverGameObject();
        if (_isMouseBeingHeld)
            IsPointerHold();
        
        // gets the position the mouse is on the ground
        var groundPosition = RaycastGround();
        if (groundPosition is null) return;
        
        // returns the current mouse position on the ground
        MousePosition.Invoke((Vector3)groundPosition);
    }
    
    private void IsPointerHold()
    {
        // if the cursor is currently over the ui, don't place buildings, as they appear under the UI
        if (_pointerOverUI) return;
        var position = RaycastGround();
        // invokes the event if the raycast was successful
        if (position.HasValue)
            OnMouseHold?.Invoke(position.Value);
    }

    private void IsPointerUp(InputAction.CallbackContext _)
    {
        _isMouseBeingHeld = false;
        OnMouseUp?.Invoke();
    }

    private void IsPointerDown(InputAction.CallbackContext _)
    {
        // if the cursor is currently over the ui, don't place buildings, as they appear under the UI
        if (_pointerOverUI) return;
        _isMouseBeingHeld = true;
        var position = RaycastGround();
        // invokes the event if the raycast was successful
        if (position.HasValue)
            OnMouseDown?.Invoke(position.Value);
    }

    private void IsRightPointerDown(InputAction.CallbackContext _)
    {
        OnRightMouseDown?.Invoke();
    }
    
    /// <summary>
    ///     Get position where the mouse touches the ground layer
    /// </summary>
    private Vector3? RaycastGround()
    {
        // create a ray from the perspective using the mouse position
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        // create a raycast from the ray, checking the ground. Returns the ground position if hit
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ground))
            return hit.point;
        return null;
    }
}

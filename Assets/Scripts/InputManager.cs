using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Action<Vector3Int> OnMouseDown { get; set; }
    public Action<Vector3Int> OnMouseHold { get; set; }
    public Action OnMouseUp { get; set; }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask ground;
    private Vector2 _cameraMovementVector;

    private void Update()
    {
        IsPointerDown();
        IsPointerUp();
        IsClickHold();
        GetInputs();

    }

    private void GetInputs()
    {
        _cameraMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    /// <summary>
    ///     Get position where the mouse touches the ground layer
    /// </summary>
    private Vector3Int? RaycastGround()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ground))
            return Vector3Int.RoundToInt(hit.point);
        return null;
    }
    
    private void IsClickHold()
    {
        if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject()) return;
        var position = RaycastGround();
        if (position is not null)
            OnMouseHold?.Invoke(position.Value);
    }

    private void IsPointerUp()
    {
        if (!Input.GetMouseButtonUp(0) || EventSystem.current.IsPointerOverGameObject()) return;
        OnMouseUp?.Invoke();
    }

    private void IsPointerDown()
    {
        if (!Input.GetMouseButtonDown(0) || EventSystem.current.IsPointerOverGameObject()) return;
        var position = RaycastGround();
        if (position is not null)
            OnMouseDown?.Invoke(position.Value);
    }
}

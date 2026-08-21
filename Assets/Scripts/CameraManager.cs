using UnityEngine;

/// <summary>
///     This class manages camera movement using the WASD keys.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementTime;
    
    // the position to lerp to every tick
    private Vector3 _newPosition;
    private bool _is3D;
    
    private void Start()
    {
        // uses the input manager to get keys directly
        inputManager.KeyboardPress += HandleKeyboardPress;
        _newPosition = transform.position;
    }

    private void LateUpdate()
    {
        // every tick, updates the camera's position to the new position using a lerp. This smoothes the movement
        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * movementTime);
    }

    private void HandleKeyboardPress(KeyboardKeys key)
    {
        // WASD are used to move the camera
        switch (key)
        {
            case KeyboardKeys.W:
                _newPosition += transform.up * (movementSpeed * Time.deltaTime);
                break;
            case KeyboardKeys.A:
                _newPosition += transform.right * (-movementSpeed * Time.deltaTime);
                break;
            case KeyboardKeys.S:
                _newPosition += transform.up * (-movementSpeed * Time.deltaTime);
                break;
            case KeyboardKeys.D:
                _newPosition += transform.right * (movementSpeed * Time.deltaTime);
                break;
        }
    }

    /// <summary>
    ///     Convert to 3D isometric view and vice versa
    /// </summary>
    public void ToggleCameraView()
    {
        if (_is3D)
        {
            // turn to 2d
            transform.rotation = Quaternion.Euler(90, 0, 0);
            _is3D = false;
        }
        else
        {
            // turn to 3d
            transform.rotation = Quaternion.Euler(45, -45, 0);
            _is3D = true;
        }
    }
}
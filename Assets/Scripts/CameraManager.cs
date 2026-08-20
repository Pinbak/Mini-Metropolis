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
}
using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementTime;

    private Vector3 _newPosition;
    
    private void Start()
    {
        inputManager.KeyboardPress += HandleKeyboardPress;
        _newPosition = transform.position;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * movementTime);
    }

    private void HandleKeyboardPress(KeyboardKeys key)
    {
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
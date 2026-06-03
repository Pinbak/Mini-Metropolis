using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;


    private void Start()
    {
        inputManager.OnMouseDown += HandleMouseClick;
    }

    private void HandleMouseClick(Vector3Int position)
    {
        Debug.Log(position);
    }

    private void Update()
    {
        
    }
}
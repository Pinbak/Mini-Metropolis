using System;
using UnityEngine;

public class PlacementHelper : MonoBehaviour
{
    public Action HoverEnter { get; set; }
    public Action HoverExit { get; set; }
    
    private Renderer _renderer;
    
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnMouseEnter()
    {
        HoverEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        HoverExit?.Invoke();
    }
}
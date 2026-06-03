using System;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private Grid _grid;
    
    private void Start()
    {
        _grid = new Grid(width, height);
    }
}
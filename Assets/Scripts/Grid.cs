using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid
{
    private Node[,] _grid;
    private int _width;
    private int _height;

    public Grid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new Node[width, height];
        
        // populate the grid with empty nodes
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            _grid[x, y] = new Node(x, y);
    }

    public Node this[int x, int y]
    {
        get => _grid[x, y];
        set => _grid[x, y] = value;
    }

    public List<(int x, int y)> GetAdjacentCells(int startingX, int startingY)
    {
        var cells = new List<(int x, int y)>();

        var currentNode = _grid[startingX, startingY];
        if (startingX > 0)
            if (_grid[startingX - 1, startingY].Type is NodeType.Empty ||
                (_grid[startingX - 1, startingY].Type is NodeType.Road && !_grid[startingX - 1, startingY].Neighbours.Contains(currentNode)))
                cells.Add((startingX - 1, startingY));
        if (startingX < _width - 1)
            if (_grid[startingX + 1, startingY].Type is NodeType.Empty ||
                (_grid[startingX + 1, startingY].Type is NodeType.Road && !_grid[startingX + 1, startingY].Neighbours.Contains(currentNode)))
                cells.Add((startingX + 1, startingY));
        if (startingY > 0)
            if (_grid[startingX, startingY - 1].Type is NodeType.Empty ||
                (_grid[startingX, startingY - 1].Type is NodeType.Road && !_grid[startingX, startingY - 1].Neighbours.Contains(currentNode)))
                cells.Add((startingX, startingY - 1));
        if (startingY < _height - 1)
            if (_grid[startingX, startingY + 1].Type is NodeType.Empty ||
                (_grid[startingX, startingY + 1].Type is NodeType.Road && !_grid[startingX, startingY + 1].Neighbours.Contains(currentNode)))
                cells.Add((startingX, startingY + 1));

        return cells;
    }
    
}

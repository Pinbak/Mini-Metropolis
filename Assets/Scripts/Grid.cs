using System.Collections.Generic;

public class Grid
{
    private NodeType[,] _grid;
    private int _width;
    private int _height;

    public Grid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new NodeType[width, height];
    }

    public NodeType this[int x, int y]
    {
        get => _grid[x, y];
        set => _grid[x, y] = value;
    }

    public List<(int x, int y)> GetAdjacentCells(int startingX, int startingY)
    {
        var cells = new List<(int x, int y)>();
        if (startingX > 0)
            cells.Add((startingX - 1, startingY));
        if (startingX < _width - 1)
            cells.Add((startingX + 1, startingY));
        if (startingY > 0)
            cells.Add((startingX, startingY - 1));
        if (startingY < _height - 1)
            cells.Add((startingX, startingY + 1));

        return cells;
    }
}

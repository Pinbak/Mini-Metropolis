using System.Collections.Generic;
using System.Linq;

public class Grid
{
    private readonly Node[,] _grid;
    private readonly int _width;
    private readonly int _height;

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

    /// <summary>
    ///     Returns a list of positions that are shared between position 1 and position 2
    /// </summary>
    public List<(int x, int y)> GetSharedNeighbours(int x1, int y1, int x2, int y2)
    {
        var adjacent1 = GetAdjacentCells(x1, y1, true);
        var adjacent2 = GetAdjacentCells(x2, y2, true);
        var overlap = adjacent1.Intersect(adjacent2);
        return overlap.ToList();
    }

    public List<(int x, int y)> GetDiagonalCells(int startingX, int startingY)
    {
        var cells = new List<(int x, int y)>();
        var node = _grid[startingX, startingY];
        var positions = new List<(int x, int y)> { (1, 1), (-1, -1), (1, -1), (-1, 1) };
        foreach (var (x, y) in positions)
        {
            var currentX = x + startingX;
            var currentY = y + startingY;
            // if out of range, continue
            if (currentX <= 0 || currentX >= _width - 1 || currentY <= 0 || currentY >= _height - 1) continue;
            var currentNode = _grid[currentX, currentY];
            // if the cell is valid, add
            if (currentNode.Type is NodeType.Empty || currentNode.Type is NodeType.Road && !currentNode.Neighbours.Contains(node))
                cells.Add((currentX, currentY));

        }

        return cells;
    }

    public List<(int x, int y)> GetAdjacentCells(int startingX, int startingY, bool ignoreStructures = false)
    {
        var cells = new List<(int x, int y)>();
        var node = _grid[startingX, startingY];
        var positions = new List<(int x, int y)>
            { (1, 0), (0, 1), (-1, 0), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1) };
        
        foreach (var (x, y) in positions)
        {
            var currentX = x + startingX;
            var currentY = y + startingY;
            // if out of range, continue
            if (currentX <= 0 || currentX >= _width - 1 || currentY <= 0 || currentY >= _height - 1) continue;
            var currentNode = _grid[currentX, currentY];
            // if the cell is valid, add
            if (ignoreStructures)
            {
                cells.Add((currentX, currentY));
                continue;
            }
            if (currentNode.Type is NodeType.Empty ||
                currentNode.Type is NodeType.Road && !currentNode.Neighbours.Contains(node))
                cells.Add((currentX, currentY));
        }
        return cells;
    }
    
}

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
}

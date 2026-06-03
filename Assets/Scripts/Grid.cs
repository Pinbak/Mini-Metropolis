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
    
    
}

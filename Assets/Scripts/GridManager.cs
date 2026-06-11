using Roads;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject road;
    private int _offsetX;
    private int _offsetY;

    public Grid Grid { get; private set; }
    public int Width => width;
    public int Height => height;
    
    public GameObject[,] Roads { get; set; } // the visual road mesh part

    private void Start()
    {
        Grid = new Grid(width, height);
        Roads = new GameObject[width, height];
        _offsetX = width / 2;
        _offsetY = height / 2;
    }
    
    public Vector2Int WorldToGrid(Vector3Int worldPosition)
    {
        return new Vector2Int(
            worldPosition.x + _offsetX,
            worldPosition.z + _offsetY
        );
    }
    
    public Vector3Int GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3Int(
            gridPosition.x - _offsetX,
            0,
            gridPosition.y - _offsetY);
    }

    public bool GridExists() => Grid is not null;

    public void CreateRoad(int x, int y, Node startNode)
    {
        if (Roads[x, y] is not null)
        {
            var currentRoad = Roads[x, y];
            var roadScript = currentRoad.GetComponent<Road>();
            roadScript.RegenerateMesh();
        }
        else
        {
            var newRoad = Instantiate(road, new Vector3(0, 0.01f, 0), Quaternion.identity);
            Roads[x, y] = newRoad;
            var newRoadScript = newRoad.GetComponent<Road>();
            newRoadScript.Initialise(startNode, this);
            newRoadScript.RegenerateMesh();
        }
    }
}
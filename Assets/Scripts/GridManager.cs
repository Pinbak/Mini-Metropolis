using Roads;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Chunk chunk;
    [SerializeField] private GameObject meshContainer;
    private int _chunkWidth;
    private int _chunkHeight;
    private int _offsetX;
    private int _offsetY;

    public Grid Grid { get; private set; }
    public int Width => width;
    public int Height => height;

    private Chunk[,] Chunks { get; set; } // the visual road mesh part

    private void Start()
    {
        Grid = new Grid(width, height);
        _chunkWidth = chunk.ChunkWidth;
        _chunkHeight = chunk.ChunkHeight;
        var chunkArrayWidth = Mathf.CeilToInt((float)width / _chunkWidth); // round up to allow for chunks that might not fit into grid perfectly
        var chunkArrayHeight = Mathf.CeilToInt((float)height / _chunkHeight);
        Chunks = new Chunk[chunkArrayWidth, chunkArrayHeight];
        _offsetX = width / 2;
        _offsetY = height / 2;
        for (var x = 0; x < chunkArrayWidth; x++)
        for (var y = 0; y < chunkArrayHeight; y++)
        {
            Chunks[x, y] =
                Instantiate(chunk, new Vector3(0, 0.01f, 0), Quaternion.identity, meshContainer.transform); // todo position is always 0
            var (start, end) = GetGridPositionFromChunk(x, y);
            Chunks[x, y].Initialise(this, start, end);
        }
        
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

    public void BuildRoadMesh(int x, int y)
    {
        var (chunkX, chunkY) = GetChunkFromGridPosition(x, y);
        
        // regenerate the mesh and update neighbours mesh
        Chunks[chunkX, chunkY].RegenerateMesh();
        Chunks[chunkX, chunkY].RegenerateNeighboursMeshes();
    }

    private (int x, int y) GetChunkFromGridPosition(int gridX, int gridY)
    {
        return (gridX / _chunkWidth, gridY / _chunkHeight);
    }

    private (int x, int y) GetGridPositionFromChunk(int chunkX, int chunkY)
    {
        return (chunkX * _chunkWidth, chunkY * _chunkHeight);
    }
}
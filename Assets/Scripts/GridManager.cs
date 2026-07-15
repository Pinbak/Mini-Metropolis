using System;
using System.Collections.Generic;
using System.Linq;
using Roads;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [field:SerializeField] public float MeshResolution { get; set; } = .2f;
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

    public bool IsWorldPositionOutsideOfGrid(Vector3 worldPosition)
    {
        var gridPosition = WorldToGrid(worldPosition);
        if (gridPosition.x >= Width || gridPosition.x < 0) return true;
        if (gridPosition.y >= height || gridPosition.y < 0) return true;
        return false;
    }

    public Node WorldToNode(Vector3 worldPosition)
    {
        var gridPosition = WorldToGrid(worldPosition);
        return Grid[gridPosition.x, gridPosition.y];
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x + _offsetX),
            Mathf.RoundToInt(worldPosition.z + _offsetY)
        );
    } 
    
    public Vector2Int WorldToGrid(Vector3Int worldPosition)
    {
        return new Vector2Int(
            worldPosition.x + _offsetX,
            worldPosition.z + _offsetY
        );
    }

    public Vector3 NodeToWorld(Node node)
    {
        return GridToWorld(node.X, node.Y);
    }

    public Vector3Int GridToWorld(int x, int y)
    {
        return new Vector3Int(
            x - _offsetX,
            0,
            y - _offsetY);
    }
    
    public Vector3Int GridToWorld(Vector2Int gridPosition) // todo check if can use above
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
        BuildChunk(chunkX, chunkY);
    }

    public void BuildChunk(int chunkX, int chunkY)
    {
        Chunks[chunkX, chunkY].RegenerateMesh();
    }

    /// <summary>
    ///     Given a list of positions, returns all unique chunks
    /// </summary>
    public List<(int chunkX, int chunkY)> GetUniqueChunksFromPositions(List<(int x, int y)> positions)
    {
        var uniqueChunks = new HashSet<(int, int)>();
        foreach (var (x, y) in positions)
        {
            var chunkPosition = GetChunkFromGridPosition(x, y);
            uniqueChunks.Add(chunkPosition);
        }

        return uniqueChunks.ToList();
    }

    private (int x, int y) GetChunkFromGridPosition(int gridX, int gridY)
    {
        return (gridX / _chunkWidth, gridY / _chunkHeight);
    }

    private (int x, int y) GetGridPositionFromChunk(int chunkX, int chunkY)
    {
        return (chunkX * _chunkWidth, chunkY * _chunkHeight);
    }

    private void OnDrawGizmos()
    {
        if (Grid is null) return;
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            switch (Grid[x, y].Type)
            {
                case NodeType.Parking:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(NodeToWorld(Grid[x, y]), .1f);
                    break;
                case NodeType.Building:
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(NodeToWorld(Grid[x, y]), .1f);
                    break;
            }
        }
    }
}
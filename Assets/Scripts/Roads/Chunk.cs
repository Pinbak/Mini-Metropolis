using System.Collections.Generic;
using Junctions;
using UnityEngine;

namespace Roads
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        [field:SerializeField] public int ChunkWidth { get; set; } = 10;
        [field:SerializeField] public int ChunkHeight { get; set; }= 10;

        // bounds of the chunk
        private int _xStart;
        private int _xEnd;
        private int _yStart;
        private int _yEnd;
        
        private GridManager _gridManager;
        private Mesh _mesh;
        private Vector3[] _vertices; // todo not used at all
        private int[] _triangles;

        // because can't have constructor in MonoBehaviour
        public void Initialise(GridManager gridManager, int xStart, int yStart)
        {
            _gridManager = gridManager;
            _xStart = xStart;
            _yStart = yStart;
            // to allow for chunks to not fit neatly into world grid
            if (xStart + ChunkWidth > gridManager.Width)
                _xEnd = gridManager.Width;
            else
                _xEnd = xStart + ChunkWidth;
            if (yStart + ChunkHeight > gridManager.Height)
                _yEnd = gridManager.Height;
            else
                _yEnd = yStart + ChunkHeight;
        }

        private void OnEnable()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        public void RegenerateMesh()
        {
            if (_mesh is null) return;
            
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var x = _xStart; x < _xEnd; x++)
            for (var y = _yStart; y < _yEnd; y++)
            {
                var node = _gridManager.Grid[x, y];
                if (node.Type is NodeType.Empty) continue;
                if (node.Neighbours.Count == 0) continue;
                
                var nodeMesh = new NodeMesh(node, _gridManager, _gridManager.MeshResolution);
                var tris = nodeMesh.Triangles;

                var startingIndex = vertices.Count;
                vertices.Add(tris[0].A1); // add the centre point

                foreach (var tri in tris)
                {
                    var localStartingIndex = vertices.Count; // starting index of specific local tri
                    vertices.Add(tri.A2); // add the left point
                    vertices.Add(tri.A3); // add the right point

                    triangles.Add(startingIndex);
                    triangles.Add(localStartingIndex);
                    triangles.Add(localStartingIndex + 1);
                }
            }
            
            _mesh.Clear();
            _mesh.vertices = vertices.ToArray();
            _mesh.triangles = triangles.ToArray();
            _mesh.RecalculateNormals();
        }

        // private void OnDrawGizmos()
        // {
        //     if (!_gridManager.GridExists()) return;
        //     for (var x = 0; x < _gridManager.Width; x++)
        //     for (var y = 0; y < _gridManager.Height; y++)
        //     {
        //         var node = _gridManager.Grid[x, y];
        //         if (node.Type is NodeType.Empty) continue;
        //         if (node.Neighbours.Count == 0) continue;
        //         var nodeMesh = new NodeMesh(node, _gridManager, _gridManager.MeshResolution);
        //         var triangles = nodeMesh.Triangles;
        //         Gizmos.color = nodeMesh.Type switch
        //         {
        //             JunctionType.DeadEnd => Color.green,
        //             JunctionType.Straight => Color.red,
        //             JunctionType.AcuteCorner => Color.blue,
        //             JunctionType.RightAngleCorner => Color.lightBlue,
        //             JunctionType.RightAngleDiagonalCorner => Color.aquamarine,
        //             JunctionType.ObtuseCorner => Color.cadetBlue,
        //             JunctionType.ComplexAcuteCorner => Color.lavenderBlush,
        //             JunctionType.ComplexCorner => Color.darkSlateBlue,
        //             JunctionType.Complex => Color.purple,
        //             _ => Gizmos.color
        //         };
        //         foreach (var triangle in triangles)
        //         {
        //             // Gizmos.DrawSphere(triangle, meshVertexSize);
        //             Gizmos.DrawLine(triangle.A1, triangle.A2);
        //             Gizmos.DrawLine(triangle.A1, triangle.A3);
        //             Gizmos.DrawLine(triangle.A2, triangle.A3);
        //             // Gizmos.color = Color.green;
        //             // Gizmos.DrawSphere(triangle.A2, meshVertexSize);
        //             // Gizmos.color = Color.blue;
        //             // Gizmos.DrawSphere(triangle.A3, meshVertexSize);
        //             // Gizmos.color = Color.red;
        //         }
        //     }
        // }

        // private void OnDrawGizmos()
        // {
        //     if (!gridManager.GridExists()) return;
        //     Gizmos.color = Color.red;
        //
        //     for (var x = 0; x < gridManager.Width; x++)
        //     for (var y = 0; y < gridManager.Height; y++)
        //     {
        //         var node = gridManager.Grid[x, y];
        //         if (node.Type is NodeType.Empty) continue;
        //         if (node.Neighbours.Count == 0) continue;
        //         var position = gridManager.GridToWorld(new Vector2Int(x, y));
        //         // Gizmos.DrawSphere(position, meshVertexSize);
        //         var nodeMesh = new NodeMesh(node, gridManager, meshResolution);
        //         var triangles = nodeMesh.Triangles;
        //         foreach (var nodeNeighbour in node.Neighbours)
        //         {
        //             var neighbourPosition = gridManager.GridToWorld(new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y));
        //             // Gizmos.DrawLine(position, neighbourPosition);
        //         }
        //
        //         Gizmos.color = nodeMesh.JunctionType switch
        //         {
        //             JunctionType.DeadEnd => Color.green,
        //             JunctionType.Straight => Color.red,
        //             JunctionType.AcuteCorner => Color.blue,
        //             JunctionType.RightAngleCorner => Color.lightBlue,
        //             JunctionType.ObtuseCorner => Color.cadetBlue,
        //             JunctionType.Complex => Color.purple,
        //             _ => Gizmos.color
        //         };
        //         // Gizmos.DrawSphere(position, .1f);
        //     
        //         foreach (var triangle in triangles)
        //         {
        //             // Gizmos.DrawSphere(triangle, meshVertexSize);
        //             Gizmos.DrawLine(triangle.A1, triangle.A2);
        //             Gizmos.DrawLine(triangle.A1, triangle.A3);
        //             Gizmos.DrawLine(triangle.A2, triangle.A3);
        //             // Gizmos.color = Color.green;
        //             // Gizmos.DrawSphere(triangle.A2, meshVertexSize);
        //             // Gizmos.color = Color.blue;
        //             // Gizmos.DrawSphere(triangle.A3, meshVertexSize);
        //             // Gizmos.color = Color.red;
        //         }
        //     }
        // }
    }
}
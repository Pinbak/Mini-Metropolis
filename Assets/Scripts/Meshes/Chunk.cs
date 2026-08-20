using System.Collections.Generic;
using UnityEngine;

namespace Meshes
{
    /// <summary>
    ///     A part of the <see cref="Grid"/>.
    /// </summary>
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

        /// <summary>
        ///     Loop through the relevant grid cells and re-generate them.
        /// </summary>
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
    }
}
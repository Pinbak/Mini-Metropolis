using System;
using System.Collections.Generic;
using Meshes;
using UnityEngine;

namespace Roads
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlacementIndicator : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        
        private PlacementMesh _placementMesh;
        private Mesh _mesh;

        private void OnEnable()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
            _placementMesh = new PlacementMesh(gridManager, gridManager.MeshResolution);
        }

        public void UpdateStartPosition(Vector3 startPosition) => _placementMesh.UpdateStartPosition(startPosition);

        public void RemoveMesh() => _mesh.Clear();
        
        public void RegenerateMesh(Vector3 currentPosition)
        {
            if (_mesh is null) return;
            
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            
            _placementMesh.CalculateTriangles(currentPosition);
            var tris = _placementMesh.Triangles;
            
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
            
            _mesh.Clear();
            _mesh.vertices = vertices.ToArray();
            _mesh.triangles = triangles.ToArray();
            _mesh.RecalculateNormals();
        }
    }
}
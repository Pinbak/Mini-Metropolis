using System.Collections.Generic;
using Meshes;
using UnityEngine;

namespace Placement
{
    /// <summary>
    ///     This is what is used to show the player where the currently placing road is going.
    ///     Utilises the <see cref="PlacementMesh"/> to visualise it.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlacementIndicator : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        
        // the actual mesh that will be used to show the player
        private PlacementMesh _placementMesh;
        private Mesh _mesh;

        private void OnEnable()
        {
            // create and init new mesh
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
            _placementMesh = new PlacementMesh(gridManager, gridManager.MeshResolution);
        }

        public void UpdateStartPosition(Vector3 startPosition) => _placementMesh.UpdateStartPosition(startPosition);
        public void RemoveMesh() => _mesh.Clear();

        public void DrawCircleAtPosition(Vector3 currentPosition)
        {
            // draw a circular mesh at the current position (which is the cursor)
            UpdateStartPosition(currentPosition);
            _placementMesh.CalculateTrianglesForCircle();
            // this is a 2-step process, where the triangles are first generated, and then turned into a mesh here
            CreateMeshFromTriangles(_placementMesh.Triangles);
        }
        
        public void DrawLineFromToPosition(Vector3 currentPosition)
        {
            _placementMesh.CalculateTrianglesFromToPosition(currentPosition);
            CreateMeshFromTriangles(_placementMesh.Triangles);
        }

        private void CreateMeshFromTriangles(List<Triangle> tris)
        {
            if (_mesh is null) return;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            
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
            _mesh.vertices = vertices.ToArray(); // Unity's built in mesh properties
            _mesh.triangles = triangles.ToArray();
            _mesh.RecalculateNormals();
        }
    }
}
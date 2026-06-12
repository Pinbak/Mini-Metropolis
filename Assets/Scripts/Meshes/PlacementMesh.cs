using System.Collections.Generic;
using UnityEngine;

namespace Meshes
{
    public class PlacementMesh : MeshGenerator
    {
        public PlacementMesh(GridManager gridManager, float resolution) : base(gridManager, resolution) { }

        public void UpdateStartPosition(Vector3 startPosition)
        {
            meshCentreInWorld = startPosition;
        }
        
        public void CalculateTriangles(Vector3 currentPosition)
        {
            Triangles = new List<Triangle>();
            var direction = currentPosition - meshCentreInWorld;
            direction.Normalize();
            var distance = Vector3.Distance(meshCentreInWorld, currentPosition);
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var left = meshCentreInWorld - perpendicular * (GlobalRoadWidth * .5f);
            left = direction * distance + left;
            var right = meshCentreInWorld + perpendicular * (GlobalRoadWidth * .5f);
            right = direction * distance + right;
            Triangles.Add(new Triangle(meshCentreInWorld, left, right));
            CreateDeadEndCap(currentPosition);
        }
    }
}
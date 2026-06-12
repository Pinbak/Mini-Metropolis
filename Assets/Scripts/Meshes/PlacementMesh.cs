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
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var left = meshCentreInWorld - perpendicular * (GlobalRoadWidth * .5f);
            left = direction * StraightRoadLength + left;
            var right = meshCentreInWorld + perpendicular * (GlobalRoadWidth * .5f);
            right = direction * StraightRoadLength + right;
            Triangles.Add(new Triangle(meshCentreInWorld, left, right));
        }
    }
}
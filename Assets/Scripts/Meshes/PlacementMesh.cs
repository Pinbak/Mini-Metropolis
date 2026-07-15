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

        public void CalculateTrianglesForCircle()
        {
            Triangles = new List<Triangle>();
            var direction = Vector3.forward * .5f - meshCentreInWorld;
            direction.Normalize();
            var newPosition = meshCentreInWorld + direction * (CapLength * .5f);
            Triangles.AddRange(CreateCap(meshCentreInWorld, newPosition, false));
            Triangles.AddRange(CreateCap(newPosition, meshCentreInWorld, false));
            Triangles.AddRange(FillTheRemainingGaps(false));
        }
        
        public void CalculateTrianglesFromToPosition(Vector3 currentPosition)
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
            Triangles.AddRange(CreateDeadEndCap(currentPosition));
            Triangles.AddRange(CreateCap(currentPosition, meshCentreInWorld, false));
            Triangles.AddRange(FillTheRemainingGaps(false));
        }
    }
}
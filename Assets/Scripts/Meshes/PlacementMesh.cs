using System.Collections.Generic;
using UnityEngine;

namespace Meshes
{
    /// <summary>
    ///     Placement mesh indicator for the player placing a road. This is the actual mesh that is visible. Inherits
    ///     from the mesh generator, to piggyback off of its methods.
    /// </summary>
    public class PlacementMesh : MeshGenerator
    {
        public PlacementMesh(GridManager gridManager, float resolution) : base(gridManager, resolution) { } // empty constructor

        public void UpdateStartPosition(Vector3 startPosition)
        {
            MeshCentreInWorld = startPosition;
        }

        /// <summary>
        ///     Calculates the mesh as a list of triangles. It stores this in <see cref="MeshGenerator.Triangles"/>
        /// </summary>
        public void CalculateTrianglesForCircle()
        {
            Triangles = new List<Triangle>();
            var direction = Vector3.forward * .5f - MeshCentreInWorld;
            direction.Normalize();
            var newPosition = MeshCentreInWorld + direction * (CapLength * .5f);
            // a circle consists of two caps and a fill between them
            Triangles.AddRange(CreateCap(MeshCentreInWorld, newPosition, false));
            Triangles.AddRange(CreateCap(newPosition, MeshCentreInWorld, false));
            Triangles.AddRange(FillTheRemainingGaps(false));
        }
        
        /// <summary>
        ///     Calculates a mesh from the start position updated in <see cref="UpdateStartPosition"/> to the current position.
        ///     Effectively draws a line between the two with a thickness and rounded ends. Used for a live preview of the
        ///     road during placement.
        /// </summary>
        public void CalculateTrianglesFromToPosition(Vector3 currentPosition)
        {
            Triangles = new List<Triangle>();
            var direction = currentPosition - MeshCentreInWorld;
            direction.Normalize();
            var distance = Vector3.Distance(MeshCentreInWorld, currentPosition);
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var left = MeshCentreInWorld - perpendicular * (GlobalRoadWidth * .5f);
            left = direction * distance + left;
            var right = MeshCentreInWorld + perpendicular * (GlobalRoadWidth * .5f);
            right = direction * distance + right;
            
            // this is also two caps but filling in the remaining gaps without a Bezier (to form a straight line)
            Triangles.Add(new Triangle(MeshCentreInWorld, left, right));
            Triangles.AddRange(CreateDeadEndCap(currentPosition));
            Triangles.AddRange(CreateCap(currentPosition, MeshCentreInWorld, false));
            Triangles.AddRange(FillTheRemainingGaps(false));
        }
    }
}
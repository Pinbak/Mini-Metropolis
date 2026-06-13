using System.Collections.Generic;
using System.Linq;
using Meshes;
using UnityEngine;

namespace Junctions
{
    /// <summary>
    ///     The visual mesh part of node
    /// </summary>
    public class NodeMesh : MeshGenerator
    {
        public int X => _node.X;
        public int Y => _node.Y;

        private JunctionType Type { get; set; }
        private readonly Node _node;
        
        public NodeMesh(Node node, GridManager gridManager, float resolution = .2f) : base(gridManager, resolution)
        {
            _node = node;
            var meshCentre = new Vector2Int(node.X, node.Y);
            meshCentreInWorld = gridManager.GridToWorld(meshCentre);
            CalculateTriangles();
        }

        private void CalculateTriangles()
        {
            Triangles = new List<Triangle>();
            foreach (var neighbour in _node.Neighbours)
            {
                var neighbourPosition = new Vector2Int(neighbour.X, neighbour.Y);
                Vector3 neighbourWorldPosition = gridManager.GridToWorld(neighbourPosition);
                var direction = neighbourWorldPosition - meshCentreInWorld;
                direction.Normalize();
                var isDiagonal = direction.x != 0 && direction.z != 0;
                var roadLength = isDiagonal ? DiagonalRoadLength : StraightRoadLength;
                var perpendicular = Vector3.Cross(Vector3.up, direction);
                var left = meshCentreInWorld - perpendicular * (GlobalRoadWidth * .5f);
                left = direction * roadLength + left;
                var right = meshCentreInWorld + perpendicular * (GlobalRoadWidth * .5f);
                right = direction * roadLength + right;
                Triangles.Add(new Triangle(meshCentreInWorld, left, right));
            }
            SortTriangles();
            var junctionType = GetJunctionType();
            Type = junctionType;
            if (junctionType is JunctionType.RightAngleCorner or JunctionType.AcuteCorner or JunctionType.ObtuseCorner
                or JunctionType.ComplexAcuteCorner or JunctionType.RightAngleDiagonalCorner or JunctionType.ComplexCorner)
                Triangles.AddRange(CreateSmoothCorners(Type));
            
            SortTriangles();
            if (junctionType == JunctionType.DeadEnd)
                // at this point, the node only has a single neighbour
                Triangles.AddRange(CreateDeadEndCap(
                    gridManager.GridToWorld(new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y))));

            SortTriangles();
            Triangles.AddRange(junctionType is JunctionType.Straight or JunctionType.DeadEnd
                ? FillTheRemainingGaps(false)
                : FillTheRemainingGaps(true));
        }
        
        /// <summary>
        ///     Returns the type of node/junction that this is
        /// </summary>
        /// <remarks>Requires triangles count to be accurate</remarks>
        private JunctionType GetJunctionType()
        {
            UpdateAveragePosition();
            return Triangles.Count switch
            {
                1 => JunctionType.DeadEnd,
                2 => IsEquilateral ? JunctionType.Straight : GetCornerType(),
                3 or 4 => GetComplexType(),
                _ => JunctionType.Complex
            };
        }

        private JunctionType GetComplexType()
        {

            var neighbourDirections = new List<Vector3>();
            var neighbourAngles = new List<float>();
            foreach (var nodeNeighbour in _node.Neighbours)
            {
                var position = new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y);
                Vector3 positionInWorld = gridManager.GridToWorld(position);
                var direction = (meshCentreInWorld - positionInWorld).normalized;
                neighbourDirections.Add(direction);
            }
            
            for (var i = 0; i < neighbourDirections.Count; i++)
            for (var j = i + 1; j < neighbourDirections.Count; j++)
            {
                var dotProduct = Vector3.Dot(neighbourDirections[i], neighbourDirections[j]);
                neighbourAngles.Add(dotProduct);
            }

            var rightAngleCount = neighbourAngles.Count(a => a >= 0);
            
            if (rightAngleCount == neighbourAngles.Count)
                return JunctionType.ComplexAcuteCorner;
            else if (rightAngleCount == neighbourAngles.Count - 1)
                return JunctionType.ComplexCorner;
            return JunctionType.Complex;
        }

        private JunctionType GetCornerType()
        {
            var aPosition = new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y);
            Vector3 aPositionInWorld = gridManager.GridToWorld(aPosition);
            var bPosition = new Vector2Int(_node.Neighbours[1].X, _node.Neighbours[1].Y);
            Vector3 bPositionInWorld = gridManager.GridToWorld(bPosition);
            var ac = (meshCentreInWorld - aPositionInWorld).normalized;
            var bc = (meshCentreInWorld - bPositionInWorld).normalized;
            var dot = Vector3.Dot(ac, bc);

            if (dot > 0)
                return JunctionType.AcuteCorner;
            if (dot < 0)
                return JunctionType.ObtuseCorner;

            // if the distance is 1, it must be non-diagonal as the cell is 1x1 in size
            if (Mathf.Approximately(Vector3.Distance(meshCentreInWorld, aPositionInWorld), 1))
                return JunctionType.RightAngleCorner;
            else
                return JunctionType.RightAngleDiagonalCorner;

        }
    }
}
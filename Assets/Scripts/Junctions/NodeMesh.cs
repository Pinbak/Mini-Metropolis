using System.Collections.Generic;
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
                or JunctionType.ComplexCorner)
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
                3 => GetComplexType(),
                _ => JunctionType.Complex
            };
        }

        private JunctionType GetComplexType()
        {
            var aPosition = new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y);
            Vector3 aPositionInWorld = gridManager.GridToWorld(aPosition);
            var bPosition = new Vector2Int(_node.Neighbours[1].X, _node.Neighbours[1].Y);
            Vector3 bPositionInWorld = gridManager.GridToWorld(bPosition);
            var cPosition = new Vector2Int(_node.Neighbours[2].X, _node.Neighbours[2].Y);
            Vector3 cPositionInWorld = gridManager.GridToWorld(cPosition);
            var ad = (meshCentreInWorld - aPositionInWorld).normalized;
            var bd = (meshCentreInWorld - bPositionInWorld).normalized;
            var cd = (meshCentreInWorld - cPositionInWorld).normalized;
            var adBd = Vector3.Dot(ad, bd);
            var bdCd= Vector3.Dot(bd, cd);
            var adCd= Vector3.Dot(ad, cd);
            if (adBd >= 0 && bdCd >= 0 && adCd >= 0)
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
            else if (dot == 0)
                return JunctionType.RightAngleCorner;
            else
                return JunctionType.ObtuseCorner;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Junctions;
using UnityEngine;

namespace Meshes
{
    /// <summary>
    ///     The visual mesh part of node. The entire grid can be made up of <see cref="NodeMesh"/> and then generated that way,
    ///     or by utilising chunks, only parts of the grid can be generated.
    /// </summary>
    public class NodeMesh : MeshGenerator
    {
        private JunctionType Type { get; set; } // what type this junction is
        private readonly Node _node; // the actual node being represented
        
        public NodeMesh(Node node, GridManager gridManager, float resolution = .2f) : base(gridManager, resolution)
        {
            _node = node;
            var meshCentre = new Vector2Int(node.X, node.Y);
            MeshCentreInWorld = gridManager.GridToWorld(meshCentre);
            CalculateTriangles(); // calculates teh triangles, which will be later used to create a mesh
        }

        private void CalculateTriangles()
        {
            Triangles = new List<Triangle>(); // store all the triangles here
            // the initial creation of the triangles from the centre of this node to all its neighbours.
            foreach (var neighbour in _node.Neighbours)
            {
                var neighbourPosition = new Vector2Int(neighbour.X, neighbour.Y);
                Vector3 neighbourWorldPosition = GridManager.GridToWorld(neighbourPosition);
                var direction = neighbourWorldPosition - MeshCentreInWorld;
                direction.Normalize();
                var isDiagonal = direction.x != 0 && direction.z != 0;
                var roadLength = isDiagonal ? DiagonalRoadLength : StraightRoadLength;
                var perpendicular = Vector3.Cross(Vector3.up, direction);
                var left = MeshCentreInWorld - perpendicular * (GlobalRoadWidth * .5f);
                left = direction * roadLength + left;
                var right = MeshCentreInWorld + perpendicular * (GlobalRoadWidth * .5f);
                right = direction * roadLength + right;
                // create a triangle from this node to the current neighbour
                Triangles.Add(new Triangle(MeshCentreInWorld, left, right));
            }
            // fan out the triangles so that the next methods work
            SortTriangles();
            var junctionType = GetJunctionType();
            Type = junctionType;
            // if the junction is a corner, the mesh for the corners are added to the triangles list 
            if (junctionType is JunctionType.RightAngleCorner or JunctionType.AcuteCorner or JunctionType.ObtuseCorner
                or JunctionType.ComplexAcuteCorner or JunctionType.RightAngleDiagonalCorner or JunctionType.ComplexCorner)
                Triangles.AddRange(CreateSmoothCorners(Type));
            
            SortTriangles();
            if (junctionType == JunctionType.DeadEnd)
                // at this point, the node only has a single neighbour
                Triangles.AddRange(CreateDeadEndCap(
                    GridManager.GridToWorld(new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y))));

            SortTriangles();
            // the remaining usually straight piece of road
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
                2 => IsEquilateral ? JunctionType.Straight : GetCornerType(), // this many neighbours means it's sometimes a corner
                3 or 4 => GetComplexType(), // this many neighbours means its complex but also usually a corner
                _ => JunctionType.Complex
            };
        }

        private JunctionType GetComplexType()
        {
            // first, generate the angles to all neighbours and store in neighbour angles
            var neighbourDirections = new List<Vector3>();
            var neighbourAngles = new List<float>();
            foreach (var nodeNeighbour in _node.Neighbours)
            {
                var position = new Vector2Int(nodeNeighbour.X, nodeNeighbour.Y);
                Vector3 positionInWorld = GridManager.GridToWorld(position);
                var direction = (MeshCentreInWorld - positionInWorld).normalized;
                neighbourDirections.Add(direction);
            }
            
            for (var i = 0; i < neighbourDirections.Count; i++)
            for (var j = i + 1; j < neighbourDirections.Count; j++)
            {
                var dotProduct = Vector3.Dot(neighbourDirections[i], neighbourDirections[j]);
                neighbourAngles.Add(dotProduct);
            }
            
            // then, count how many of the neighbours are right angles, this can be used to determine junction type
            var rightAngleCount = neighbourAngles.Count(a => a >= 0);
            
            // based on that information, junction type can be determined
            if (rightAngleCount == neighbourAngles.Count)
                return JunctionType.ComplexAcuteCorner;
            else if (rightAngleCount == neighbourAngles.Count - 1)
                return JunctionType.ComplexCorner;
            return JunctionType.Complex;
        }

        private JunctionType GetCornerType()
        {
            // given 2 neighbours, this checks to see how they are placed
            var aPosition = new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y);
            Vector3 aPositionInWorld = GridManager.GridToWorld(aPosition);
            var bPosition = new Vector2Int(_node.Neighbours[1].X, _node.Neighbours[1].Y);
            Vector3 bPositionInWorld = GridManager.GridToWorld(bPosition);
            var ac = (MeshCentreInWorld - aPositionInWorld).normalized;
            var bc = (MeshCentreInWorld - bPositionInWorld).normalized;
            var dot = Vector3.Dot(ac, bc);

            if (dot > 0)
                return JunctionType.AcuteCorner;
            if (dot < 0)
                return JunctionType.ObtuseCorner;

            // if the distance is 1, it must be non-diagonal as the cell is 1x1 in size
            if (Mathf.Approximately(Vector3.Distance(MeshCentreInWorld, aPositionInWorld), 1))
                return JunctionType.RightAngleCorner;
            else
                return JunctionType.RightAngleDiagonalCorner;

        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Junctions
{
    /// <summary>
    ///     The visual mesh part of node
    /// </summary>
    public class NodeMesh
    {
        public List<Triangle> Triangles { get; set; }
        private JunctionType Type => GetJunctionType();
        private bool IsEquilateral => AveragePosition == _nodeCentreInWorld; // if the junction is straight or has connections evenly spaced
        private Vector3 AveragePosition => UpdateAveragePosition(); // the average position of all the triangles

        private readonly Node _node;
        private readonly GridManager _gridManager; // reference to the grid manager for spacial related information
        private readonly Vector2Int _nodeCentre;
        private readonly Vector3 _nodeCentreInWorld;
        
        private readonly float _straightRoadLength = .5f;
        private readonly float _diagonalRoadLength = .7071f;
        private readonly float _globalRoadWidth = .3f;
        private readonly float _cornerLength = .3f;
        private readonly float _meshResolution = .2f;
        private readonly float _capLength = .3f;
        private readonly float _curviness = .3f;

        public NodeMesh(Node node, GridManager gridManager)
        {
            _node = node;
            _nodeCentre = new Vector2Int(node.X, node.Y);
            _gridManager = gridManager;
            _nodeCentreInWorld = _gridManager.GridToWorld(_nodeCentre);
            CalculateTriangles();
        }
        
        public void CalculateTriangles()
        {
            Triangles = new List<Triangle>();
            foreach (var neighbour in _node.Neighbours)
            {
                var neighbourPosition = new Vector2Int(neighbour.X, neighbour.Y);
                Vector3 neighbourWorldPosition = _gridManager.GridToWorld(neighbourPosition);
                var direction = neighbourWorldPosition - _nodeCentreInWorld;
                direction.Normalize();
                var isDiagonal = direction.x != 0 && direction.z != 0;
                var roadLength = isDiagonal ? _diagonalRoadLength : _straightRoadLength;
                var perpendicular = Vector3.Cross(Vector3.up, direction);
                var left = _nodeCentreInWorld - perpendicular * (_globalRoadWidth * .5f);
                left = direction * roadLength + left;
                var right = _nodeCentreInWorld + perpendicular * (_globalRoadWidth * .5f);
                right = direction * roadLength + right;
                Triangles.Add(new Triangle(_nodeCentreInWorld, left, right));
            }
            SortTriangles();
            var junctionType = Type;
            if (junctionType == JunctionType.Corner)
                Triangles.AddRange(CreateSmoothCorners());
            
            SortTriangles();
            if (junctionType == JunctionType.DeadEnd)
                Triangles.AddRange(CreateDeadEndCap());

            SortTriangles();
            Triangles.AddRange(CreateMissingTriangles());
        }

        /// <summary>
        ///     Fills in the gaps
        /// </summary>
        private List<Triangle> CreateMissingTriangles()
        {
            var triangles = new List<Triangle>();
            var numberOfTriangles = Triangles.Count;
            for (var i = 0; i < numberOfTriangles; i++)
            {
                var a = Triangles[i].A3;
                var b = Triangles[(i + 1) % numberOfTriangles].A2;
                var movedCentre = Vector3.Lerp((a + b) / 2, _nodeCentreInWorld, _curviness);
                triangles.AddRange(GenerateTrianglesFromBezierPoints(a, b, movedCentre));
            }

            return triangles;
        }

        private List<Triangle> CreateDeadEndCap()
        {
            var neighbourPosition = new Vector2Int(_node.Neighbours[0].X, _node.Neighbours[0].Y);
            Vector3 neighbourWorldPosition = _gridManager.GridToWorld(neighbourPosition);
            var direction = neighbourWorldPosition - _nodeCentreInWorld;
            direction.Normalize();
            direction *= -1; // flip the direction
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var a = _nodeCentreInWorld - perpendicular * (_globalRoadWidth * .5f);
            var b = _nodeCentreInWorld + perpendicular * (_globalRoadWidth * .5f);
            var c = _nodeCentreInWorld + direction * _capLength;

            return GenerateTrianglesFromBezierPoints(a, b, c);
        }

        private List<Triangle> CreateSmoothCorners()
        {
            var direction = AveragePosition - _nodeCentreInWorld;
            direction.Normalize();
            direction *= -1;
            var cornerPosition = _nodeCentreInWorld + direction * _cornerLength;
            var angle = Vector3.SignedAngle(_nodeCentreInWorld.normalized, direction, Vector3.up);

            // getting where this position is within the fan of triangles
            var insertIndex = 0;
            while(insertIndex < Triangles.Count)
            {
                var triDirection = Triangles[insertIndex].Centre - _nodeCentreInWorld;
                var triAngle = Vector3.SignedAngle(_nodeCentreInWorld.normalized, triDirection.normalized,
                    Vector3.up);
                if (angle < triAngle)
                    break;
                insertIndex++;
            }
            
            var previous = (insertIndex - 1 + Triangles.Count) % Triangles.Count;
            var next = insertIndex % Triangles.Count;

            var prevTri = Triangles[previous];
            var nextTri = Triangles[next];

            return GenerateTrianglesFromBezierPoints(prevTri.A3, nextTri.A2, cornerPosition);
        }

        /// <summary>
        ///     Given <see cref="BezierCurve"/> points, a, b, centre, creates a curve using <see cref="_meshResolution"/>,
        ///     and then generating triangles from centre
        /// </summary>
        private List<Triangle> GenerateTrianglesFromBezierPoints(Vector3 a, Vector3 b, Vector3 centre)
        {
            var tris = new List<Triangle>();
            var bezierPoints = new List<Vector3>();
            for (var t = _meshResolution; t < 1f; t += _meshResolution)
            {
                var point = BezierCurve.EvaluateQuadratic(a, centre, b, t);
                bezierPoints.Add(point);
            }
                
            for (var i = 0; i < bezierPoints.Count; i++)
            {
                // start of the bezier
                if (i == 0)
                {
                    tris.Add(new Triangle(_nodeCentreInWorld, a, bezierPoints[i]));
                }
                // all points in the bezier
                if (i != bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(_nodeCentreInWorld, bezierPoints[i], bezierPoints[i + 1]));
                }
                // last point of the bezier
                else if (i == bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(_nodeCentreInWorld, bezierPoints[i], b));
                }
            }

            return tris;
        }

        /// <summary>
        ///     Sorts the <see cref="Triangles"/> list so that the triangles "fan" out clockwise
        /// </summary>
        private void SortTriangles()
        {
            Triangles.Sort((a, b) =>
            {
                var aDir = a.Centre - _nodeCentreInWorld;
                var bDir = b.Centre - _nodeCentreInWorld;

                var angleA = Vector3.SignedAngle(_nodeCentreInWorld.normalized, aDir.normalized, Vector3.up);
                var angleB = Vector3.SignedAngle(_nodeCentreInWorld.normalized, bDir.normalized, Vector3.up);

                if (angleA > angleB)
                    return 1;
                if (angleA < angleB)
                    return -1;
                return 0;
            });
        }
        
        /// <summary>
        ///     Returns the type of node/junction that this is
        /// </summary>
        /// <remarks>Requires triangles count to be accurate</remarks>
        private JunctionType GetJunctionType()
        {
            return Triangles.Count switch
            {
                1 => JunctionType.DeadEnd,
                2 => IsEquilateral ? JunctionType.Straight : JunctionType.Corner,
                _ => JunctionType.Complex
            };
        }

        /// <summary>
        ///     Updates <see cref="AveragePosition"/> to be the average of all the <see cref="Triangles"/> centre
        /// </summary>
        private Vector3 UpdateAveragePosition()
        {
            var averagePosition = new Vector3();
            foreach (var triangle in Triangles)
            {
                averagePosition += triangle.Centre;
            }
            averagePosition /= Triangles.Count;
            return averagePosition;
        }
        
    }
}
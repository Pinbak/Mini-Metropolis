using System.Collections.Generic;
using Junctions;
using UnityEngine;

namespace Meshes
{
    public class MeshGenerator
    {
        public List<Triangle> Triangles { get; protected set; }
        
        protected bool IsEquilateral => _averagePosition == meshCentreInWorld; // if the junction is straight or has connections evenly spaced

        private Vector3 _averagePosition; // the average position of all the triangles
        
        protected readonly GridManager gridManager; // reference to the grid manager for spacial related information
        protected Vector3 meshCentreInWorld;
        
        protected const float StraightRoadLength = .5f;
        protected const float DiagonalRoadLength = .7071f;
        protected const float GlobalRoadWidth = .4f;
        private const float AcuteCornerLength = .6f;
        private const float RightAngleCornerDiagonalLength = .45f;
        private const float RightAngleCornerLength = .3f;
        private const float ObtuseCornerLength = .25f;
        private const float ComplexCornerLength = .2f;
        protected const float CapLength = .3f;
        private const float Curviness = .25f;
        private readonly float _meshResolution;

        protected MeshGenerator(GridManager gridManager, float resolution)
        {
            this.gridManager = gridManager;
            _meshResolution = resolution;
        }
        
        protected List<Triangle> FillTheRemainingGaps(bool useBezier)
        {
            var triangles = new List<Triangle>();
            var numberOfTriangles = Triangles.Count;
            for (var i = 0; i < numberOfTriangles; i++)
            {
                var a = Triangles[i].A3;
                var b = Triangles[(i + 1) % numberOfTriangles].A2;
                if (a == b)
                    continue;
                if (useBezier)
                {
                    var movedCentre = Vector3.Lerp((a + b) / 2, meshCentreInWorld, Curviness);
                    triangles.AddRange(GenerateTrianglesFromBezierPoints(a, b, movedCentre, meshCentreInWorld));
                }
                else
                {
                    triangles.Add(new Triangle(meshCentreInWorld, a, b));
                }
                
            }

            return triangles;
        }
        
        protected List<Triangle> CreateDeadEndCap(Vector3 neighbourPosition, bool isPointed = false)
            => CreateCap(meshCentreInWorld, neighbourPosition, isPointed);

        protected List<Triangle> CreateCap(Vector3 start, Vector3 end, bool isPointed)
        {
            var direction = end - start;
            direction.Normalize();
            direction *= -1; // flip the direction
            var perpendicular = Vector3.Cross(Vector3.up, direction);
            var a = start - perpendicular * (GlobalRoadWidth * .5f);
            var b = start + perpendicular * (GlobalRoadWidth * .5f);
            var c = start + direction * CapLength;
            // creates a rounded Bezier end cap
            if (!isPointed) return GenerateTrianglesFromBezierPoints(a, b, c, start);
            
            // creates a simple pointed end cap
            var tris = new List<Triangle>
            {
                new(start, a, c),
                new(start, c, b)
            };
            return tris;
            
        }
        
        protected List<Triangle> CreateSmoothCorners(JunctionType junctionType)
        {
            var direction = _averagePosition - meshCentreInWorld;
            direction.Normalize();
            direction *= -1;
            var cornerLength = ObtuseCornerLength;
            
            if (junctionType is JunctionType.AcuteCorner)
                cornerLength = AcuteCornerLength;
            if (junctionType is JunctionType.RightAngleCorner)
                cornerLength = RightAngleCornerLength;
            if (junctionType is JunctionType.RightAngleDiagonalCorner or JunctionType.ComplexAcuteCorner)
                cornerLength = RightAngleCornerDiagonalLength;
            if (junctionType is JunctionType.ComplexCorner)
                cornerLength = ComplexCornerLength;
            
            var cornerPosition = meshCentreInWorld + direction * cornerLength;
            var angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.up);
            
            // getting where this position is within the fan of triangles
            var insertIndex = 0;
            while(insertIndex < Triangles.Count)
            {
                var triDirection = Triangles[insertIndex].Centre - meshCentreInWorld;
                var triAngle = Vector3.SignedAngle(Vector3.right, triDirection.normalized,
                    Vector3.up);
                if (angle < triAngle)
                    break;
                insertIndex++;
            }
            
            var previous = (insertIndex - 1 + Triangles.Count) % Triangles.Count;
            var next = insertIndex % Triangles.Count;

            var prevTri = Triangles[previous];
            var nextTri = Triangles[next];

            var a = prevTri.A3;
            var b = nextTri.A2;

            return GenerateTrianglesFromBezierPoints(a, b, cornerPosition, meshCentreInWorld);
        }

        /// <summary>
        ///     Given <see cref="BezierCurve"/> points, a, b, centre, creates a curve using <see cref="_meshResolution"/>,
        ///     and then generating triangles from centre
        /// </summary>
        private List<Triangle> GenerateTrianglesFromBezierPoints(Vector3 a, Vector3 b, Vector3 bezierControlPoint, Vector3 start)
        {
            var tris = new List<Triangle>();
            var bezierPoints = new List<Vector3>();
            for (var t = _meshResolution; t < 1f; t += _meshResolution)
            {
                var point = BezierCurve.EvaluateQuadratic(a, bezierControlPoint, b, t);
                bezierPoints.Add(point);
            }
                
            for (var i = 0; i < bezierPoints.Count; i++)
            {
                // start of the bezier
                if (i == 0)
                {
                    tris.Add(new Triangle(start, a, bezierPoints[i]));
                }
                // all points in the bezier
                if (i != bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(start, bezierPoints[i], bezierPoints[i + 1]));
                }
                // last point of the bezier
                else if (i == bezierPoints.Count - 1)
                {
                    tris.Add(new Triangle(start, bezierPoints[i], b));
                }
            }

            return tris;
        }

        /// <summary>
        ///     Sorts the <see cref="Triangles"/> list so that the triangles "fan" out clockwise
        /// </summary>
        protected void SortTriangles()
        {
            Triangles.Sort((a, b) =>
            {
                var aDir = a.Centre - meshCentreInWorld;
                var bDir = b.Centre - meshCentreInWorld;

                var angleA = Vector3.SignedAngle(Vector3.right, aDir.normalized, Vector3.up);
                var angleB = Vector3.SignedAngle(Vector3.right, bDir.normalized, Vector3.up);

                if (angleA > angleB)
                    return 1;
                if (angleA < angleB)
                    return -1;
                return 0;
            });
        }
        
        /// <summary>
        ///     Updates <see cref="_averagePosition"/> to be the average of all the <see cref="Triangles"/> centre
        /// </summary>
        protected void UpdateAveragePosition()
        {
            var totalPosition = new Vector3();
            foreach (var triangle in Triangles)
            {
                totalPosition += triangle.Centre;
            }
            totalPosition /= Triangles.Count;
            _averagePosition = totalPosition;
        }
    }
}
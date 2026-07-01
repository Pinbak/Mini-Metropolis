using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    /// <summary>
    ///     Converts an A* found path of <see cref="Node"/>s into a <see cref="Vector3"/> path to be followed by a car
    /// </summary>
    public class PathGenerator
    {
        public List<Vector3> Path { get; private set; } = new();
        public bool PathGenerated { get; private set; }
        
        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathInset = .2f;
        private const float PathWidth = .4f; // todo get from elsewhere
        private const float PathStraightLength = .5f; // todo get from elsewhere
        private const float PathDiagonalLength = .7071f; // todo get from elsewhere
        private const float TurnSmoothness = .1f; // lower number is smoother

        public PathGenerator(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        /// <summary>
        ///     Gets the next position in the path to visit, <see cref="foundPosition"/> returns true if a position exists
        /// </summary>
        public Vector3 GetNextPosition(Vector3 currentPosition, out bool foundPosition)
        {
            VisitedPosition(currentPosition);
            if (Path.Count == 0)
            {
                foundPosition = false;
                return Vector3.zero;
            }
            foundPosition = true;
            return Path[0];
        }
        
        /// <summary>
        ///     After visiting this position, call <see cref="VisitedPosition"/>
        ///     Updates <see cref="PathGenerated"/> and <see cref="Path"/>
        /// </summary>
        private void VisitedPosition(Vector3 position)
        {
            Path.Remove(position);
            if (Path.Count != 0) return;
            // if the car has reached the destination
            PathGenerated = false;
        }
        
        public void GeneratePath(List<Node> nodePath)
        {
            Path = new List<Vector3>();

            for (var i = 1; i < nodePath.Count - 1; i++)
            {
                var position = _gridManager.GridToWorld(new Vector2Int(nodePath[i].X, nodePath[i].Y));
                var nextPosition = _gridManager.GridToWorld(new Vector2Int(nodePath[i + 1].X, nodePath[i + 1].Y));
                var previousPosition = _gridManager.GridToWorld(new Vector2Int(nodePath[i - 1].X, nodePath[i - 1].Y));
                
                // todo add the ability to swap road sides
                Vector3 directionToNextPosition = nextPosition - position;
                directionToNextPosition.Normalize();
                var nextPerpendicular = Vector3.Cross(Vector3.up, directionToNextPosition);
                var nextPoint = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f);
                var nextIsDiagonal = directionToNextPosition.x != 0 && directionToNextPosition.z != 0;
                var nextRoadLength = nextIsDiagonal ? PathDiagonalLength : PathStraightLength;

                // todo just repeated from above
                Vector3 directionToPreviousPosition = previousPosition - position;
                directionToPreviousPosition.Normalize();
                var previousPerpendicular = Vector3.Cross(Vector3.up, directionToPreviousPosition);
                var previousPoint = position - previousPerpendicular * ((PathWidth - PathInset) * .5f); // using negative as is opposite direction
                var previousIsDiagonal = directionToPreviousPosition.x != 0 && directionToPreviousPosition.z != 0;
                var previousRoadLength = previousIsDiagonal ? PathDiagonalLength : PathStraightLength;
                
                nextPoint = directionToNextPosition * (nextRoadLength) + nextPoint;
                previousPoint = directionToPreviousPosition * (previousRoadLength) + previousPoint;
                var movedPosition = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f); // the centre of the node that is shifted to the correct lane
                
                
                // for straight roads, we don't need to Bezier
                if (directionToNextPosition == directionToPreviousPosition * -1)
                    Path.Add(movedPosition);
                else
                {
                    for (var t = TurnSmoothness; t < 1f; t += TurnSmoothness)
                    {
                        var point = BezierCurve.EvaluateQuadratic(previousPoint, movedPosition, nextPoint, t);
                        Path.Add(point);
                    }

                }

            }

            PathGenerated = Path.Count != 0;

        }
    }
}
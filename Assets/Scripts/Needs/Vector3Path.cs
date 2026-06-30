using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class Vector3Path
    {
        // Converts a path of nodes into a viable path of vector3 points to follow
        public List<Vector3> Path { get; private set; } = new();
        public bool PathGenerated { get; private set; }
        
        private Node _currentPosition;
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathInset = .2f;
        private const float PathWidth = .4f; // todo get from elsewhere
        private const float PathStraightLength = .5f; // todo get from elsewhere
        private const float PathDiagonalLength = .7071f; // todo get from elsewhere
        private const float TargetTolerance = .01f;
        private const float TurnSmoothness = .1f; // lower number is smoother

        public Vector3Path(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        public void MoveAlongPath(GameObject objectToMove, float movementSpeed)
        {
            if (!PathGenerated) return;

            var currentPosition = objectToMove.transform.position;
            var currentRotation = objectToMove.transform.rotation;
            var rotationSpeed = movementSpeed * 10f;

            if (Vector3.Distance(currentPosition, _currentTargetPosition) > TargetTolerance)
            {
                // if the car are not yet at the target, move it and rotate it
                objectToMove.transform.position =
                    Vector3.MoveTowards(currentPosition, _currentTargetPosition, movementSpeed * Time.deltaTime);
                var direction = _currentTargetPosition - currentPosition;
                var targetAngle = Vector3.SignedAngle(Vector3.right, direction.normalized, Vector3.up);
                var targetRotation = Quaternion.Euler(0, targetAngle, 0);
                objectToMove.transform.rotation =
                    Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // if the car has reached the target, it needs to get the next target
                Path.Remove(_currentTargetPosition);
                if (Path.Count == 0)
                {
                    // if the car have reached the destination
                    PathGenerated = false;
                    return;
                }
                _currentTargetPosition = Path[0];
                
            }
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
            if (PathGenerated) _currentTargetPosition = Path[0];

        }
        
    }
}
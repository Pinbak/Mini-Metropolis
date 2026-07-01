using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class PathMover
    {
        public bool PathExists => _pathGenerator.PathGenerated;
        public IReadOnlyCollection<Vector3> Path => _pathGenerator.Path.AsReadOnly();
        public Node CurrentPosition => _pathGenerator.CurrentNode; // the node that the car is currently on
        public Node NextPosition => _pathGenerator.NextNode; // the node that the car is moving to
        
        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;

        public PathMover(GridManager gridManager, Node startPosition)
        {
            _pathGenerator = new PathGenerator(gridManager, startPosition);
        }

        public void GeneratePath(Node end)
        {
            _pathGenerator.GeneratePath(CurrentPosition, end);
            if (_pathGenerator.PathGenerated) _currentTargetPosition = _pathGenerator.Path[0];
        }

        public void MoveAlongPath(GameObject objectToMove, float movementSpeed)
        {
            if (!PathExists) return;

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
                _currentTargetPosition = _pathGenerator.GetNextPosition();
            }
        }
        
    }
}
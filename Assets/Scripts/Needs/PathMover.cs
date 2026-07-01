using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class PathMover
    {
        public bool PathExists => _pathGenerator.PathGenerated;
        public IReadOnlyCollection<Vector3> Path => _pathGenerator.Path.AsReadOnly();
        public Node CurrentPosition => _pathGenerator.CurrentNode; // the node that the agent is currently on
        public Node NextPosition => _pathGenerator.NextNode; // the node that the agent is moving to

        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;
        private readonly GameObject _agent; // a reference to the object this is affecting

        public PathMover(GridManager gridManager, GameObject agent)
        {
            _pathGenerator = new PathGenerator(gridManager, agent);
            _agent = agent;
        }

        public void UpdateCurrentNodeFromWorldPosition(Vector3 worldPosition) =>
            _pathGenerator.UpdateCurrentNodeFromWorldPosition(worldPosition);

        public void GeneratePath(Node end)
        {
            _pathGenerator.GeneratePath(CurrentPosition, end);
            if (_pathGenerator.PathGenerated) _currentTargetPosition = _pathGenerator.Path[0];
        }

        public void MoveAlongPath(float movementSpeed)
        {
            if (!PathExists) return;

            var currentPosition = _agent.transform.position;
            var currentRotation = _agent.transform.rotation;
            var rotationSpeed = movementSpeed * 10f;

            if (Vector3.Distance(currentPosition, _currentTargetPosition) > TargetTolerance)
            {
                // if the agent is not yet at the target, move it and rotate it
                _agent.transform.position =
                    Vector3.MoveTowards(currentPosition, _currentTargetPosition, movementSpeed * Time.deltaTime);
                var direction = _currentTargetPosition - currentPosition;
                var targetAngle = Vector3.SignedAngle(Vector3.right, direction.normalized, Vector3.up);
                var targetRotation = Quaternion.Euler(0, targetAngle, 0);
                _agent.transform.rotation =
                    Quaternion.Lerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // if the agent has reached the target, it needs to get the next target
                _currentTargetPosition = _pathGenerator.GetNextPosition();
            }
        }
        
    }
}
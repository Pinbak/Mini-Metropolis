using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class PathMover
    {
        public bool HasValidPath { get; private set; }
        public IReadOnlyCollection<Vector3> Path => _pathGenerator.Path.AsReadOnly();
        public Node CurrentPosition { get; private set; } // the node that the agent is currently on
        public Node NextPosition { get; private set; } // the node that the agent is moving to

        private int _currentNodePointer;
        private int _currentPositionPointer;

        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;
        private readonly GameObject _agent; // a reference to the object this is affecting
        private readonly GridManager _gridManager;

        public PathMover(GridManager gridManager, GameObject agent)
        {
            _pathGenerator = new PathGenerator(gridManager, agent);
            _agent = agent;
            _gridManager = gridManager;
            UpdateCurrentNodeFromWorldPosition(agent.transform.position);
        }
        
        /// <summary>
        ///     If the agent's position has been moved in the editor, the <see cref="CurrentPosition"/> will be out of sync,
        ///     this syncs it up
        /// </summary>
        public void UpdateCurrentNodeFromWorldPosition(Vector3 worldPosition)
        {
            CurrentPosition = _gridManager.WorldToNode(worldPosition);
        }

        public void GeneratePath(Node end)
        {
            _currentNodePointer = 0;
            _currentPositionPointer = 0;
            _pathGenerator.GeneratePath(CurrentPosition, end);
            if (_pathGenerator.PathGenerated)
            {
                _currentTargetPosition = _pathGenerator.Path[0];
                HasValidPath = true;
            }
        }

        public void MoveAlongPath(float movementSpeed)
        {
            if (!HasValidPath) return;

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
                _currentTargetPosition = GetNextPosition();
            }
        }
        
        /// <summary>
        ///     Gets the next position in the path to visit
        /// </summary>
        private Vector3 GetNextPosition() // todo it's a bit of a mess
        {
            _currentPositionPointer++;
            if (_currentPositionPointer == Path.Count)
            {
                _currentPositionPointer = 0;
                CurrentPosition = _pathGenerator.NodePath[_currentNodePointer];
                _currentNodePointer++;
                if (_currentNodePointer == _pathGenerator.NodePath.Length)
                {
                    _currentNodePointer = 0;
                    HasValidPath = false;
                    // we have reached our destination
                    return Vector3.zero;
                }
                NextPosition = _pathGenerator.NodePath[_currentNodePointer];
                // the road has been removed since setting out
                if (NextPosition.Type is not NodeType.Road)
                {
                    _currentNodePointer = 0;
                    HasValidPath = false;
                    return Vector3.zero;
                }
                _pathGenerator.GenerateSteps(_currentNodePointer);
            }
            return _pathGenerator.Path[_currentPositionPointer];
        }
        
    }
}
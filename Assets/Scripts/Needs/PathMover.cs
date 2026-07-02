using System.Collections.Generic;
using Intersections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Needs
{
    public class PathMover
    {
        public bool HasValidPath { get; private set; }
        public IReadOnlyCollection<Vector3> Path => _pathGenerator.Path.AsReadOnly();
        public Node CurrentPosition { get; private set; } // the node that the agent is currently on
        public Node NextPosition { get; private set; } // the node that the agent is moving to
        public bool MovingInJunction { get; set; }

        private int _currentNodePointer;
        private int _currentPositionPointer;
        private bool _attemptRecalculatePath; // if path breaks try once to recalculate
        private Node _destination;
        private readonly LayerMask _agentLayer;
        private readonly float _detectionDistance = .8f; // the distance to check for other agents

        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;
        private readonly GameObject _agent; // a reference to the object this is affecting
        private readonly GridManager _gridManager;
        private readonly IntersectionManager _intersectionManager;
        private float _currentSpeed;
        private float _speedMultiplier = 1f; // used to stop the agent
        private const float Acceleration = 1f;
        private const float DistanceToAgentInFront = 0.5f; // how close the agent gets to another agent before fully stopping

        public PathMover(GridManager gridManager, IntersectionManager intersectionManager, GameObject agent,
            LayerMask agentLayer)
        {
            _pathGenerator = new PathGenerator(gridManager, agent);
            _agent = agent;
            _agentLayer = agentLayer;
            _gridManager = gridManager;
            _intersectionManager = intersectionManager;
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
            _destination = end;
            if (_pathGenerator.PathGenerated)
            {
                _currentTargetPosition = _pathGenerator.Path[0];
                HasValidPath = true;
            }
        }

        public void Go()
        {
            MovingInJunction = true;
            _speedMultiplier = 1f;
        }

        public void Stop()
        {
            _speedMultiplier = 0f;
        }

        public void MoveAlongPath(float movementSpeed)
        {
            if (!HasValidPath) return;

            var currentPosition = _agent.transform.position;
            var distanceToNextNode = Vector3.Distance(currentPosition, _currentTargetPosition);
            if (distanceToNextNode > TargetTolerance)
            {
                var currentRotation = _agent.transform.rotation;
                var rotationSpeed = movementSpeed * 10f;
                var adjustedSpeed = movementSpeed;
                var acceleration = Acceleration;
                if (!MovingInJunction)
                {
                    adjustedSpeed = movementSpeed * _speedMultiplier;
                    if (Physics.Raycast(currentPosition, _agent.transform.forward, out var hit, _detectionDistance,
                            _agentLayer))
                    {

                        Debug.DrawLine(new Vector3(currentPosition.x, currentPosition.y + 0.1f, currentPosition.z),
                            hit.point, Color.blue);
                        adjustedSpeed = movementSpeed * Mathf.Max(0f, hit.distance - DistanceToAgentInFront);
                        // make acceleration/deceleration inversely proportional to distance
                        acceleration = Mathf.Min(Acceleration,
                            Acceleration + Mathf.Pow(Acceleration * 2f - hit.distance * 2f, 4));
                    }
                }
                
                
                // slowly change the speed to match the maximum target speed * the distance to the car in front
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, adjustedSpeed, acceleration * Time.deltaTime);
                
                // if the agent is not yet at the target, move it and rotate it
                _agent.transform.position =
                    Vector3.MoveTowards(currentPosition, _currentTargetPosition, _currentSpeed * Time.deltaTime);
                var direction = _currentTargetPosition - currentPosition;
                var targetAngle = Vector3.SignedAngle(Vector3.forward, direction.normalized, Vector3.up);
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
                if (_intersectionManager.IsIntersection(NextPosition))
                    _intersectionManager.AddToIntersection(this, NextPosition);
                // the road has been removed since setting out
                if (NextPosition.Type is not NodeType.Road)
                {
                    _currentNodePointer = 0;
                    HasValidPath = false;
                    // if (!_attemptRecalculatePath)
                    // {
                    //     _attemptRecalculatePath = true;
                    //     UpdateCurrentNodeFromWorldPosition(_agent.transform.position);
                    //     GeneratePath(_destination);
                    // }
                    return Vector3.zero;
                }
                _pathGenerator.GenerateSteps(_currentNodePointer);
            }
            return _pathGenerator.Path[_currentPositionPointer];
        }
        
    }
}
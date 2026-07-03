using System;
using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
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
        public Vector3 WorldPosition => _agent.transform.position;
        public ParkingSpace ParkedAt { get; private set; }// the space this agent is currently in
        public ParkingSpace Destination { get; private set; }
        public Action<Node> Arrived { get; set; } // invoked when the agent has arrived at its intended destination
        
        private Vector3 _targetNodePosition;
        private int _currentNodePointer;
        private int _currentPositionPointer;
        private bool _attemptRecalculatePath; // if path breaks try once to recalculate
        private readonly LayerMask _agentLayer;
        private readonly float _detectionDistance = .8f; // the distance to check for other agents
        private bool _approachingJunction;

        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;
        private readonly GameObject _agent; // a reference to the object this is affecting
        private readonly GridManager _gridManager;
        private readonly IntersectionManager _intersectionManager;
        private float _currentSpeed;
        private float _speedMultiplier = 1f; // used to stop the agent
        private float _acceleration = 1f;
        private const float DistanceToAgentInFront = .3f; // how close the agent gets to another agent before fully stopping
        private BuildingInformation _buildingInformation; // the building that this car belongs to

        public PathMover(BuildingInformation buildingInformation, GridManager gridManager,
            IntersectionManager intersectionManager, GameObject agent,
            LayerMask agentLayer, ParkingSpace initialParking)
        {
            _pathGenerator = new PathGenerator(gridManager, this);
            _agent = agent;
            _agentLayer = agentLayer;
            _gridManager = gridManager;
            _intersectionManager = intersectionManager;
            _buildingInformation = buildingInformation;
            ParkedAt = initialParking;
            CurrentPosition = gridManager.WorldToNode(initialParking.ParentPosition);
        }
        
        /// <summary>
        ///     If the agent's position has been moved in the editor, the <see cref="CurrentPosition"/> will be out of sync,
        ///     this syncs it up
        /// </summary>
        public void UpdateCurrentNodeFromPosition(Vector3 worldPosition)
        {
            CurrentPosition = _gridManager.WorldToNode(worldPosition);
        }

        public void GeneratePath(ParkingSpace parkingSpace)
        {
            if (ParkedAt is null) return;
            Destination = parkingSpace;
            var startingPosition = _gridManager.WorldToNode(ParkedAt.RoadConnection);
            var parkingSpaceNode = _gridManager.WorldToNode(parkingSpace.RoadConnection);
            var actualPosition = _gridManager.WorldToNode(ParkedAt.ParentPosition);
            var actualGoal = _gridManager.WorldToNode(parkingSpace.ParentPosition);
            GeneratePath(actualPosition, actualGoal, startingPosition, parkingSpaceNode);
        }

        private void GeneratePath(Node modifiedStart, Node modifiedEnd, Node start, Node end)
        {
            _currentNodePointer = 0;
            _currentPositionPointer = 0;
            _pathGenerator.GeneratePath(modifiedStart, modifiedEnd, start, end);
            if (_pathGenerator.PathGenerated)
            {
                ParkedAt.IsFree = true;
                ParkedAt.IsBeingTaken = false;
                Destination.IsBeingTaken = true; // todo not the best place, as when a road is removed, the space will never be freed, also blocks a space even when no path is found
                ParkedAt = null;
                _currentTargetPosition = _pathGenerator.Path[0];
                HasValidPath = true;
            }
        }

        public void Go()
        {
            MovingInJunction = true;
            _speedMultiplier = 1f;
            _approachingJunction = false;
        }

        public void Stop()
        {
            _speedMultiplier = 0f;
            _approachingJunction = true;
        }

        public void MoveAlongPath(float movementSpeed, AnimationCurve accelerationProfile)
        {
            if (!HasValidPath) return;
            if (ParkedAt is not null) return; // we're currently parked

            var currentPosition = WorldPosition;
            var distanceToNextStep = Vector3.Distance(currentPosition, _currentTargetPosition);
            if (distanceToNextStep > TargetTolerance)
            {
                var currentRotation = _agent.transform.rotation;
                var rotationSpeed = movementSpeed * 10f;
                var adjustedSpeed = movementSpeed;
                var acceleration = _acceleration;
                if (!MovingInJunction)
                {
                    if (_speedMultiplier == 0f)
                    {
                        adjustedSpeed = movementSpeed * _speedMultiplier;
                    }
                    else
                    {
                        if (Physics.Raycast(currentPosition, _agent.transform.forward, out var hit, _detectionDistance,
                                _agentLayer))
                        {
                            if (Vector3.Dot(_agent.transform.forward, hit.transform.forward) > 0)
                            {
                                Debug.DrawLine(new Vector3(currentPosition.x, currentPosition.y + 0.1f, currentPosition.z),
                                    hit.point, Color.blue);
                                adjustedSpeed = movementSpeed * Mathf.Max(0f, hit.distance - DistanceToAgentInFront);
                                // make acceleration/deceleration inversely proportional to distance
                                acceleration = accelerationProfile.Evaluate(hit.distance);
                            }
                        }
                    }
                    
                }

                if (_approachingJunction)
                {
                    var distanceToJunction = Vector3.Distance(currentPosition, _targetNodePosition);
                    acceleration = accelerationProfile.Evaluate(Mathf.Min(1f, distanceToJunction));
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

        private Vector3 ParkUp()
        {
            return ParkedAt.transform.position;
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
                    // if (Vector3.Distance(WorldPosition, ParkedAt.transform.position) > TargetTolerance)
                    //     return ParkUp();
                    _currentNodePointer = 0;
                    HasValidPath = false;
                    // we have reached the adjacent road
                    ParkedAt = Destination;
                    ParkedAt.IsFree = false;
                    _agent.transform.rotation = Destination.transform.rotation;
                    Arrived?.Invoke(CurrentPosition);
                    return Vector3.zero;

                }
                NextPosition = _pathGenerator.NodePath[_currentNodePointer];
                _targetNodePosition = _gridManager.NodeToWorld(NextPosition);
                if (_intersectionManager.IsIntersection(NextPosition))
                    _intersectionManager.AddToIntersection(this, NextPosition);
                // the road has been removed since setting out
                if (NextPosition.Type is not NodeType.Road && NextPosition.Type is not NodeType.Parking)
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
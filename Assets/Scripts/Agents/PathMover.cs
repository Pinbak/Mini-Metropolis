using System;
using System.Collections.Generic;
using Buildings;
using Junctions;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Agents
{
    /// <summary>
    ///     A path mover is the part of the agent responsible for moving along a path. It can handle path generation, and
    ///     transforming itself to move along a path. It also handles junctions and road interactions with other agents.
    /// </summary>
    [Serializable]
    public class PathMover
    {
        [field:SerializeField] public bool HasValidPath { get; private set; }
        public IReadOnlyCollection<Vector3> Path => _pathGenerator.Path.AsReadOnly();
        public Node CurrentPosition { get; private set; } // the node that the agent is currently on
        public Node NextPosition { get; private set; } // the node that the agent is moving to
        [field:SerializeField] public bool MovingInJunction { get; set; }
        public Vector3 WorldPosition => _agent.transform.position;
        public Action<Node> Arrived { get; set; } // invoked when the agent has arrived at its intended destination
        public ParkingSpace Destination { get; private set; }
        public ParkingSpace ParkedAt
        {
            get => _parkedAt;
            private set
            {
                if (value is null) _parkedAt.Leave();
                else value.Park(this);
                _parkedAt = value;
            }
        } // the space this agent is currently in
        
        private int _currentNodePointer;
        private int _currentPositionPointer;
        private bool _attemptRecalculatePath; // if path breaks try once to recalculate
        private readonly LayerMask _agentLayer;
        private readonly float _detectionDistance = .8f; // the distance to check for other agents

        // Converts a path of nodes into a viable path of vector3 points to follow
        private readonly PathGenerator _pathGenerator; // the vector3 generator to create points along the path of nodes to be followed
        private Vector3 _currentTargetPosition; // the position that we are currently driving towards
        private const float TargetTolerance = .01f;
        private readonly Agent _agent; // a reference to the object this is affecting
        private readonly GridManager _gridManager;
        private readonly JunctionManager _junctionManager;
        private float _currentSpeed;
        private float _speedMultiplier = 1f; // used to stop the agent
        private float _acceleration = 1f;
        private const float DistanceToAgentInFront = .3f; // how close the agent gets to another agent before fully stopping
        private Building _buildingInformation; // the building that this car belongs to
        private ParkingSpace _parkedAt;
        private const float TeleportHomeRetryTime = 20f;
        [SerializeField] private float teleportHomeTimer; // if the agent gets stuck in a traffic jam too long, it will teleport to its primary location
        private Vector3 _lastPosition;
        private Junction _lastJunctionVisited;

        /// <summary>
        ///     Create a new path mover
        /// </summary>
        /// <param name="buildingInformation">The building that this car belongs to</param>
        /// <param name="gridManager">The global grid manager object</param>
        /// <param name="junctionManager">The global junction manager object</param>
        /// <param name="agent">The agent which this path mover will affect. This agent's transform will be modified</param>
        /// <param name="agentLayer">The layer the agents are on for collision checking</param>
        /// <param name="initialParking">Where the agent is currently parked</param>
        public PathMover(Building buildingInformation, GridManager gridManager,
            JunctionManager junctionManager, Agent agent,
            LayerMask agentLayer, ParkingSpace initialParking)
        {
            _pathGenerator = new PathGenerator(gridManager, this);
            _agent = agent;
            _agentLayer = agentLayer;
            _gridManager = gridManager;
            _junctionManager = junctionManager;
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

        /// <summary>
        ///     Generate a path from current position to the parkingSpace.
        /// </summary>
        /// <param name="parkingSpace">The parking space to generate the path to. Note. uses current position as starting point</param>
        public void GeneratePath(ParkingSpace parkingSpace)
        {
            if (ParkedAt is null) return;
            Destination = parkingSpace;
            // using the parking space information, the relevant positions can be gathered for generating the path and steps
            var startingPosition = _gridManager.WorldToNode(ParkedAt.RoadConnection);
            var parkingSpaceNode = _gridManager.WorldToNode(parkingSpace.RoadConnection);
            var actualPosition = _gridManager.WorldToNode(ParkedAt.ParentPosition);
            var actualGoal = _gridManager.WorldToNode(parkingSpace.ParentPosition);
            
            // actually generating a path
            GeneratePath(actualPosition, actualGoal, startingPosition, parkingSpaceNode);
        }

        private void GeneratePath(Node modifiedStart, Node modifiedEnd, Node start, Node end)
        {
            // reset values so the agent will move afresh
            _currentNodePointer = 0;
            _currentPositionPointer = 0;
            teleportHomeTimer = 0f;
            _pathGenerator.GeneratePath(modifiedStart, modifiedEnd, start, end);
            if (_pathGenerator.PathGenerated)
            {
                ParkedAt = null;
                Destination.Reserve(this);
                _currentTargetPosition = _pathGenerator.Path[0];
                HasValidPath = true;
                // flip the agent to begin moving forwards
                _agent.transform.RotateAround(_agent.transform.position, _agent.transform.up, 180f);
                Go();
            }
        }

        /// <summary>
        ///     Continue moving.
        /// </summary>
        public void Go()
        {
            _speedMultiplier = 1f;
        }

        /// <summary>
        ///     Stop moving.
        /// </summary>
        public void Stop()
        {
            _speedMultiplier = 0f;
        }

        /// <summary>
        ///     Updates the agent's transform to reflect a movement along the path that was generated with <see cref="GeneratePath"/>.
        /// </summary>
        /// <param name="movementSpeed">The speed to move at</param>
        /// <param name="accelerationProfile">The acceleration and deceleration to use</param>
        public void MoveAlongPath(float movementSpeed, AnimationCurve accelerationProfile)
        {
            if (!HasValidPath) return;
            if (ParkedAt is not null) return; // this agent is currently parked
            
            // if the agent is stuck in the same place for too long, it will teleport to its primary location, but
            if (teleportHomeTimer > TeleportHomeRetryTime)
            {
                TeleportToPrimaryAndRemoveFromJunction();
            }
            if (Vector3.Distance(_lastPosition, _agent.transform.position) < .01f)
            {
                teleportHomeTimer += Time.deltaTime;
            }
            else
            {
                // if the agent has moved, reset the timer
                teleportHomeTimer = 0f;
            }

            var currentPosition = WorldPosition;
            var distanceToNextStep = Vector3.Distance(currentPosition, _currentTargetPosition);
            // if not at next step yet (using a tolerance as floating-point precision isn't accurate enough to use equality comparison)
            if (distanceToNextStep > TargetTolerance)
            {
                var currentRotation = _agent.transform.rotation;
                var rotationSpeed = movementSpeed * 10f;
                var adjustedSpeed = movementSpeed;
                var acceleration = _acceleration;
                // if currently stopped, for example, at a junction, reduce speed
                if (_speedMultiplier == 0f)
                {
                    adjustedSpeed = movementSpeed * _speedMultiplier;
                }
                else
                {
                    // otherwise, raycast other agents to check if need to slow down before of another agent blocking the way
                    if (Physics.Raycast(currentPosition, _agent.transform.forward, out var hit, _detectionDistance,
                            _agentLayer))
                    {
                        // if the agent is facing a similar direction
                        if (Vector3.Dot(_agent.transform.forward, hit.transform.forward) > 0)
                        {
                            Debug.DrawLine(new Vector3(currentPosition.x, currentPosition.y + 0.1f, currentPosition.z),
                                hit.point, Color.blue);
                            // adjust speed based on the raycasts distance
                            adjustedSpeed = movementSpeed * Mathf.Max(0f, hit.distance - DistanceToAgentInFront);
                            // make acceleration/deceleration inversely proportional to distance
                            acceleration = accelerationProfile.Evaluate(hit.distance);
                        }
                    }
                }
                
                // slowly change the speed to match the maximum target speed * the distance to the car in front
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, adjustedSpeed, acceleration * Time.deltaTime);
                
                // if the agent is not yet at the target, move it and rotate it
                _lastPosition = _agent.transform.position;
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
        ///     Teleports the agent to its primary position, resetting any relevant properties.
        /// </summary>
        private void TeleportToPrimary()
        {
            if (_agent.AgentState == _agent.AtPrimary) return; // if already at primary
            if (!_agent.PrimaryLocation.GetFreeParkingSpace(out var parkingSpace)) return;
            teleportHomeTimer = 0f;
            _currentNodePointer = 0;
            HasValidPath = false;
            ParkedAt = parkingSpace;
            Destination?.Leave();
            _agent.transform.rotation = parkingSpace.transform.rotation;
            _agent.transform.position = parkingSpace.transform.position;
            CurrentPosition = _gridManager.WorldToNode(parkingSpace.ParentPosition);
            _agent.Returning();
            _agent.ChangeState(_agent.AtPrimary);
        }

        public void TeleportToPrimaryAndRemoveFromJunction()
        {
            TeleportToPrimary();
            _lastJunctionVisited?.RemoveAgentFromQueue(this);
        }
        
        public void PrepareForDeletion()
        {
            // this frees parking spaces etc. and removes the agent from the junction it may be waiting at, removing their place from the queue
            TeleportToPrimaryAndRemoveFromJunction();
        }
        
        private void ArrivedAtLocation()
        {
            _currentNodePointer = 0;
            HasValidPath = false;
            // park up
            ParkedAt = Destination;
            _agent.transform.rotation = Destination.transform.rotation;
            Arrived?.Invoke(CurrentPosition); // used for changing state
        }
        
        /// <summary>
        ///     Gets the next position in the path to visit
        /// </summary>
        private Vector3 GetNextPosition()
        {
            // increment step to get the next position to go to
            _currentPositionPointer++;
            // if that was the final step in the node, get the first step of the next node
            if (_currentPositionPointer == Path.Count)
            {
                _currentPositionPointer = 0;
                CurrentPosition = _pathGenerator.NodePath[_currentNodePointer];
                _currentNodePointer++;
                // if this is the final node, then the agent has arrived
                if (_currentNodePointer == _pathGenerator.NodePath.Length)
                {
                    ArrivedAtLocation();
                    return Vector3.zero;
                }
                
                // otherwise, the next steps can be gathered from the next node
                NextPosition = _pathGenerator.NodePath[_currentNodePointer];
                // if this new node puts the agent in a junction, it will need to register there, handing over control
                // to the selected junction
                if (_junctionManager.IsJunction(NextPosition)) 
                    _lastJunctionVisited = _junctionManager.AddToJunctionQueue(this, NextPosition);
                // the road has been removed since setting out
                if (NextPosition.Type is not NodeType.Road && NextPosition.Type is not NodeType.Parking)
                {
                    TeleportToPrimaryAndRemoveFromJunction();
                    return Vector3.zero;
                }
                // generate steps for the new node to follow
                _pathGenerator.GenerateSteps(_currentNodePointer);
            }
            return _pathGenerator.Path[_currentPositionPointer];
        }
    }
}
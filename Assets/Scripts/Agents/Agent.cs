using Buildings;
using UnityEngine;

namespace Agents
{
    /// <summary>
    ///     An agent is an entity that exists which implements a finite-state machine and pathfinding capabilities.
    ///     Agents interact with one another through the <see cref="PathMover"/> control.
    /// </summary>
    public class Agent : MonoBehaviour
    {
        // Initial setup values used in the agent's prefab
        [field:SerializeField] public AgentType AgentType { get; set; }
        [field:SerializeField] public int Income { get; set; }
        [field:SerializeField] public float MovementSpeed { get; set; } = 1f;
        [field:SerializeField] public float RequestTime { get; set; }
        [field:SerializeField] public float TimeToSpendAtSecondary { get; set; }
        [field:SerializeField] public float TimeToWaitUntilRetryingRoute { get; set; } = 5f;
        [field:SerializeField] public float NeedIncrease { get; set; } = 250f;
        [field:SerializeField] public float UpgradeAmount { get; set; } = 1000f;
        [field:SerializeField] public float DowngradeAmount { get; set; }
        [field:SerializeField] public bool InQueue { get; set; } // queueing for need

        // States
        public IAgentState AgentState { get; private set; }
        public AtPrimary AtPrimary { get; private set; } = new();
        public AtSecondary AtSecondary { get; private set; } = new();
        public TravellingToPrimary TravellingToPrimary { get; private set; } = new();
        public TravellingToSecondary TravellingToSecondary { get; private set; } = new();
        
        public Building PrimaryLocation { get; private set; }
        public Building SecondaryLocation { get; private set; }
        [field:SerializeField] public PathMover PathMover { get; private set; } // the ability to move along a path
        public AnimationCurve CarAcceleration { get; private set; }
        
        private GridManager _gridManager;
        protected BuildingManager BuildingManager { get; set; } // parent

        public void Init(Building primaryLocation, BuildingManager buildingManager, ParkingSpace initialParkingSpace)
        {
            _gridManager = buildingManager.GridManager;
            BuildingManager = buildingManager;
            AgentState = AtPrimary;
            AgentState.EnterState(this);
            CarAcceleration = buildingManager.CarAcceleration;
            PrimaryLocation = primaryLocation;
            PathMover = new PathMover(primaryLocation, _gridManager, buildingManager.JunctionManager, this,
                buildingManager.AgentLayer, initialParkingSpace);
        }

        /// <summary>
        ///     Send this agent to a location
        /// </summary>
        public void GoTo(Building location)
        {
            InQueue = false;
            SecondaryLocation = location;
        }
        
        /// <summary>
        ///     If the agent has no <see cref="SecondaryLocation"/>, it is logically returning or at its
        ///     <see cref="PrimaryLocation"/>.
        /// </summary>
        public void Returning()
        {
            SecondaryLocation = null;
        }

        /// <summary>
        ///     Transition to another state.
        /// </summary>
        public void ChangeState(IAgentState newState)
        {
            AgentState.ExitState(this);
            AgentState = newState;
            AgentState.EnterState(this);
        }

        private void Update()
        {
            // depending on state, the agent's action differ
            AgentState.Update(this);
        }
        
        /// <summary>
        ///     Debug draw information about the agents current whereabouts and function.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (PathMover is null) return;
            if (!PathMover.HasValidPath) return;
            // return;
            
            Gizmos.color = Color.red;
            
            foreach (var node in PathMover.Path)
            {
                Gizmos.DrawSphere(new Vector3(node.x, 0.2f, node.z), 0.1f);
            }

            Gizmos.color = Color.blue;
            var currentPosition =
                _gridManager.GridToWorld(new Vector2Int(PathMover.CurrentPosition.X, PathMover.CurrentPosition.Y));
            var nextPosition =
                _gridManager.GridToWorld(new Vector2Int(PathMover.NextPosition.X, PathMover.NextPosition.Y));
            Gizmos.DrawSphere(new Vector3(currentPosition.x, 0.2f, currentPosition.z), 0.1f);
            Gizmos.color = Color.purple;
            Gizmos.DrawSphere(new Vector3(nextPosition.x, 0.2f, nextPosition.z), 0.1f);
        }
    }
}
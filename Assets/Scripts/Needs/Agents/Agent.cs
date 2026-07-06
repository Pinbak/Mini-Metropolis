using System;
using Needs.Buildings;
using UnityEngine;

namespace Needs.Agents
{
    public class Agent : MonoBehaviour
    {
        public IAgentState AgentState { get; private set; }
        [field:SerializeField] public float MovementSpeed { get; set; }= 1f;
        [field:SerializeField] public float TimeToSpendAtPrimary { get; set; }
        [field:SerializeField] public float TimeToSpendAtSecondary { get; set; }
        
        // States
        public AtPrimary AtPrimary { get; private set; } = new();
        public AtSecondary AtSecondary { get; private set; } = new();
        public TravellingToPrimary TravellingToPrimary { get; private set; } = new();
        public TravellingToSecondary TravellingToSecondary { get; private set; } = new();
        
        public Building PrimaryLocation { get; private set; }
        public Building SecondaryLocation { get; private set; }
        public PathMover PathMover { get; private set; } // the ability to move along a path
        public AnimationCurve CarAcceleration { get; private set; }
        
        private GridManager _gridManager;
        private BuildingManager _buildingManager; // parent

        public void Init(Building primaryLocation, Building secondaryLocation,
            BuildingManager buildingManager, ParkingSpace initialParkingSpace)
        {
            _gridManager = buildingManager.GridManager;
            _buildingManager = buildingManager;
            AgentState = AtPrimary;
            AgentState.EnterState(this);
            CarAcceleration = buildingManager.CarAcceleration;
            PrimaryLocation = primaryLocation;
            SecondaryLocation = secondaryLocation;
            PathMover = new PathMover(primaryLocation, _gridManager, buildingManager.IntersectionManager, gameObject,
                buildingManager.AgentLayer, initialParkingSpace);
        }

        public void ChangeState(IAgentState newState)
        {
            AgentState.ExitState(this);
            AgentState = newState;
            AgentState.EnterState(this);
        }

        private void Update()
        {
            AgentState.Update(this); // todo might be null as update runs before init once??
        }
        
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
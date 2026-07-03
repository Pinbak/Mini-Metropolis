using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needs.Agents
{
    public class Commuter : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = .5f;
        private AnimationCurve _carAcceleration;
        private PathMover _pathMover; // the ability to move along a path
        private GridManager _gridManager; // the grid that this car sits on
        private BuildingManager _buildingManager; // parent
        private IntersectionManager _intersectionManager;
        private Industrial _workplace;

        private void Update()
        {
            _pathMover.MoveAlongPath(movementSpeed, _carAcceleration);
            // return;
            // if (!_pathMover.HasValidPath)
            // {
            //     var validGoalPositions = new List<Node>();
            //     for (var x = 0; x < _gridManager.Width; x++)
            //     for (var y = 0; y < _gridManager.Height; y++)
            //     {
            //         var node = _gridManager.Grid[x, y];
            //         if (node.Type is NodeType.Road && !_intersectionManager.IsIntersection(node))
            //             validGoalPositions.Add(node);
            //     }
            //
            //     if (validGoalPositions.Count == 0) return;
            //     var newGoalNode = validGoalPositions[Random.Range(0, validGoalPositions.Count)];
            //     _pathMover.GeneratePath(newGoalNode);
            // }
        }

        public void Init(BuildingInformation buildingInformation, BuildingManager buildingManager, GridManager gridManager,
            IntersectionManager intersectionManager, AnimationCurve carAcceleration, ParkingSpace initialParkingSpace)
        {
            _gridManager = gridManager;
            _buildingManager = buildingManager;
            _intersectionManager = intersectionManager;
            _carAcceleration = carAcceleration;
            _pathMover = new PathMover(buildingInformation, gridManager, intersectionManager, gameObject,
                buildingManager.AgentLayer, initialParkingSpace);
        }

        public void FindTestPath()
        {
            // if (!_workplace.BuildingInformation.CheckParkingIsFree()) return;
            _workplace = _buildingManager.TestIndustrial;
            var parkingSpace = _workplace.BuildingInformation.Park(_pathMover);
            if (parkingSpace is null) return;
            _pathMover.GeneratePath(parkingSpace);
        }

        private void OnDrawGizmos()
        {
            if (_pathMover is null) return;
            if (!_pathMover.HasValidPath) return;
            // return;
            
            Gizmos.color = Color.red;
            
            foreach (var node in _pathMover.Path)
            {
                Gizmos.DrawSphere(new Vector3(node.x, 0.2f, node.z), 0.1f);
            }

            Gizmos.color = Color.blue;
            var currentPosition =
                _gridManager.GridToWorld(new Vector2Int(_pathMover.CurrentPosition.X, _pathMover.CurrentPosition.Y));
            var nextPosition =
                _gridManager.GridToWorld(new Vector2Int(_pathMover.NextPosition.X, _pathMover.NextPosition.Y));
            Gizmos.DrawSphere(new Vector3(currentPosition.x, 0.2f, currentPosition.z), 0.1f);
            Gizmos.color = Color.purple;
            Gizmos.DrawSphere(new Vector3(nextPosition.x, 0.2f, nextPosition.z), 0.1f);
            
        }
    }
}
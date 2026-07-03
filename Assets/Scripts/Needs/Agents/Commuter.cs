using System.Collections.Generic;
using Intersections;
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

        private void Update()
        {
            _pathMover.MoveAlongPath(movementSpeed, _carAcceleration);
            // return;
            if (!_pathMover.HasValidPath)
            {
                var validGoalPositions = new List<Node>();
                for (var x = 0; x < _gridManager.Width; x++)
                for (var y = 0; y < _gridManager.Height; y++)
                {
                    var node = _gridManager.Grid[x, y];
                    if (node.Type is NodeType.Road && !_intersectionManager.IsIntersection(node))
                        validGoalPositions.Add(node);
                }
            
                if (validGoalPositions.Count == 0) return;
                var newGoalNode = validGoalPositions[Random.Range(0, validGoalPositions.Count)];
                _pathMover.GeneratePath(newGoalNode);
            }
        }

        public void Init(BuildingManager buildingManager, GridManager gridManager,
            IntersectionManager intersectionManager, AnimationCurve carAcceleration, Node startingPosition)
        {
            _gridManager = gridManager;
            _buildingManager = buildingManager;
            _intersectionManager = intersectionManager;
            _carAcceleration = carAcceleration;
            _pathMover = new PathMover(gridManager, intersectionManager, gameObject, buildingManager.AgentLayer,
                startingPosition);
        }

        public void FindTestPath()
        {
            var goalNode = _gridManager.WorldToNode(_buildingManager.TestPosition.transform.position);
            
            _pathMover.UpdateCurrentNodeFromPosition(transform.position); // just in case the car has been moved in the editor
            _pathMover.GeneratePath(goalNode);
        }

        private void OnDrawGizmos()
        {
            if (_pathMover is null) return;
            if (!_pathMover.HasValidPath) return;
            return;
            
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
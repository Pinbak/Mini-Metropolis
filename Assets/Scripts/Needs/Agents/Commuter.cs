using System;
using System.Collections.Generic;
using Intersections;
using Needs.Buildings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Needs.Agents
{
    public class Commuter : MonoBehaviour
    {
        public Action ArrivedAtWork { get; set; }
        public Action ArrivedHome { get; set; }

        [field: SerializeField] public State Currently { get; set; } = State.AtHome;
        
        [SerializeField] private float movementSpeed = .5f;
        [SerializeField] private float spendAtWork = 10f;
        private float _spentAtWork;
        private AnimationCurve _carAcceleration;
        private PathMover _pathMover; // the ability to move along a path
        private GridManager _gridManager; // the grid that this car sits on
        private BuildingManager _buildingManager; // parent
        private IntersectionManager _intersectionManager;
        private Industrial _workplace;
        private Residential _home;

        private void Update()
        {
            _pathMover.MoveAlongPath(movementSpeed, _carAcceleration);
            if (Currently is State.AtWork)
            {
                _spentAtWork += Time.deltaTime;
                if (_spentAtWork > spendAtWork)
                    GoHome();
            }
        }

        public void Init(Residential home, BuildingManager buildingManager, GridManager gridManager,
            IntersectionManager intersectionManager, AnimationCurve carAcceleration, ParkingSpace initialParkingSpace)
        {
            _gridManager = gridManager;
            _buildingManager = buildingManager;
            _intersectionManager = intersectionManager;
            _carAcceleration = carAcceleration;
            _home = home;
            _pathMover = new PathMover(home, gridManager, intersectionManager, gameObject,
                buildingManager.AgentLayer, initialParkingSpace);
            _pathMover.Arrived += Arrived;
        }
        
        private void GoHome()
        {
            if (_pathMover.HasValidPath) Debug.Log("Attempting to travel home while travelling");
            if (!_home.GetFreeParkingSpace(out var parkingSpace)) return;
            _pathMover.GeneratePath(parkingSpace);
            if (_pathMover.HasValidPath)
                Currently = State.Travelling;
        }

        public void GoToWork()
        {
            if (_pathMover.HasValidPath) Debug.Log("Attempting to travel to work while travelling");
            _workplace = _buildingManager.TestIndustrial; // todo temporary
            if (_workplace is null) return;
            if (!_workplace.GetFreeParkingSpace(out var parkingSpace)) return;
            _pathMover.GeneratePath(parkingSpace);
            if (_pathMover.HasValidPath)
                Currently = State.Travelling;
        }

        private void Arrived(Node node)
        {
            if (_pathMover.CurrentPosition == _workplace.BottomLeft) // todo technically true, although not if car park was over to the right
            {
                _spentAtWork = 0;
                Currently = State.AtWork;
                ArrivedAtWork?.Invoke();
            }
            else
            {
                Currently = State.AtHome;
                ArrivedHome?.Invoke();
            }
        }
        

        #region Draw Gizmos

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
        
        #endregion
    }
}
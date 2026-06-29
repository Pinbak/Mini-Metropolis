using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class Worker : MonoBehaviour
    {
        private Pathfinding _pathfinding;
        private List<Node> _currentPath = new();
        private GridManager _gridManager;
        private CarManager _carManager;

        public void Init(CarManager carManager, GridManager gridManager)
        {
            _gridManager = gridManager;
            _carManager = carManager;
            _pathfinding = new Pathfinding(gridManager.Grid);
        }

        public void FindTestPath()
        {
            var positionOnGrid = _gridManager.WorldToGrid(transform.position);
            var currentNode = _gridManager.Grid[positionOnGrid.x, positionOnGrid.y];
            var goalPositionOnGrid = _gridManager.WorldToGrid(_carManager.TestPosition.transform.position);
            var goalNode = _gridManager.Grid[goalPositionOnGrid.x, goalPositionOnGrid.y];
            _currentPath = _pathfinding.FindPath(currentNode, goalNode);
        }

        private void OnDrawGizmos()
        {
            if (_currentPath.Count == 0) return;
            
            Gizmos.color = Color.red;
            
            foreach (var node in _currentPath)
            {
                var position = _gridManager.GridToWorld(new Vector2Int(node.X, node.Y));
                Gizmos.DrawSphere(position, 0.1f);
            }

            for (var i = 0; i < _currentPath.Count - 1; i++)
            {
                var start = _gridManager.GridToWorld(new Vector2Int(_currentPath[i].X, _currentPath[i].Y));
                var end = _gridManager.GridToWorld(new Vector2Int(_currentPath[i + 1].X, _currentPath[i + 1].Y));
                Gizmos.DrawLine(new Vector3(start.x, .1f, start.z), new Vector3(end.x, .1f, end.z));
            }
            
        }
    }
}
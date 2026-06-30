using UnityEngine;

namespace Needs
{
    public class Worker : MonoBehaviour
    {
        private Pathfinding _pathfinding;
        private Vector3Path _vector3Path;
        private Node _currentPosition;
        private GridManager _gridManager;
        private CarManager _carManager;

        public void Init(CarManager carManager, GridManager gridManager)
        {
            _gridManager = gridManager;
            _carManager = carManager;
            _pathfinding = new Pathfinding(gridManager.Grid);
            _vector3Path = new Vector3Path(gridManager);
        }

        public void FindTestPath()
        {
            var positionOnGrid = _gridManager.WorldToGrid(transform.position);
            var currentNode = _gridManager.Grid[positionOnGrid.x, positionOnGrid.y];
            var goalPositionOnGrid = _gridManager.WorldToGrid(_carManager.TestPosition.transform.position);
            var goalNode = _gridManager.Grid[goalPositionOnGrid.x, goalPositionOnGrid.y];
            _pathfinding.GeneratePath(currentNode, goalNode);
            _vector3Path.GeneratePath(_pathfinding.Path);
        }

        private void OnDrawGizmos()
        {
            if (_vector3Path.Path.Count == 0) return;
            if (!_pathfinding.ValidPathExists) return;
            
            Gizmos.color = Color.red;
            
            foreach (var node in _vector3Path.Path)
            {
                Gizmos.DrawSphere(node, 0.1f);
            }

            for (var i = 0; i < _vector3Path.Path.Count - 1; i++)
            {
                Gizmos.DrawLine(_vector3Path.Path[i], _vector3Path.Path[i + 1]);
            }
            
        }
    }
}
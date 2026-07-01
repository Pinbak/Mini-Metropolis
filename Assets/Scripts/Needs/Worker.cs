using UnityEngine;

namespace Needs
{
    public class Worker : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = .5f;
        private PathMover _pathMover; // the ability to move along a path
        private GridManager _gridManager; // the grid that this car sits on
        private CarManager _carManager; // parent

        private void Update()
        {
            _pathMover.MoveAlongPath(gameObject, movementSpeed);
        }

        public void Init(CarManager carManager, GridManager gridManager)
        {
            _gridManager = gridManager;
            _carManager = carManager;
            var positionOnGrid = _gridManager.WorldToGrid(transform.position);
            var currentNode = _gridManager.Grid[positionOnGrid.x, positionOnGrid.y];
            _pathMover = new PathMover(gridManager, currentNode);
        }

        public void FindTestPath()
        {
            
            var goalPositionOnGrid = _gridManager.WorldToGrid(_carManager.TestPosition.transform.position);
            var goalNode = _gridManager.Grid[goalPositionOnGrid.x, goalPositionOnGrid.y];

            _pathMover.GeneratePath(goalNode);
        }

        private void OnDrawGizmos()
        {
            if (!_pathMover.PathExists) return;
            
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
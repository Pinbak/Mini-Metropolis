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
            _pathMover = new PathMover(gridManager);
        }

        public void FindTestPath()
        {
            var positionOnGrid = _gridManager.WorldToGrid(transform.position);
            var currentNode = _gridManager.Grid[positionOnGrid.x, positionOnGrid.y];
            var goalPositionOnGrid = _gridManager.WorldToGrid(_carManager.TestPosition.transform.position);
            var goalNode = _gridManager.Grid[goalPositionOnGrid.x, goalPositionOnGrid.y];

            _pathMover.GeneratePath(currentNode, goalNode);
        }

        private void OnDrawGizmos()
        {
            if (!_pathMover.PathExists) return;
            
            Gizmos.color = Color.red;
            
            foreach (var node in _pathMover.Path)
            {
                Gizmos.DrawSphere(new Vector3(node.x, 0.2f, node.z), 0.1f);
            }

            // for (var i = 0; i < _vector3Path.Path.Count - 1; i++)
            // {
            //     Gizmos.DrawLine(_vector3Path.Path[i], _vector3Path.Path[i + 1]);
            // }
            
        }
    }
}
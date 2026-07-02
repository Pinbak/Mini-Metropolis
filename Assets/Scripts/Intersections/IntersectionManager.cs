using System.Collections.Generic;
using Needs;
using UnityEngine;

namespace Intersections
{
    public class IntersectionManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LayerMask agentLayer;

        private Intersection[,] _intersections;
        private readonly List<Intersection> _activeIntersections = new();

        private void Start()
        {
            _intersections = new Intersection[gridManager.Width, gridManager.Height];
        }

        private void Update()
        {
            foreach (var activeIntersection in _activeIntersections)
            {
                activeIntersection.Process();
            }
        }

        public void AddToIntersection(PathMover agentToAdd, Node intersectionToAddTo)
        {
            var intersection = _intersections[intersectionToAddTo.X, intersectionToAddTo.Y];
            intersection.AddToQueue(agentToAdd);
        }
        
        /// <summary>
        ///     Checks if the node is an intersection or not. Returns true if it is
        /// </summary>
        public bool IsIntersection(Node node)
        {
            return _intersections[node.X, node.Y] is not null;
        }
        
        public void CreateIntersection(Node node)
        {
            var x = node.X;
            var y = node.Y;
            if (_intersections[x, y] is not null) return;
            var worldPosition = gridManager.GridToWorld(x, y);
            var newIntersection = new Intersection(node, worldPosition, agentLayer);
            _intersections[x, y] = newIntersection;
            _activeIntersections.Add(newIntersection);
        }

        public void RemoveIntersection(int x, int y)
        {
            if (_intersections[x, y] is null) return;
            var intersection = _intersections[x, y];
            _activeIntersections.Remove(intersection);
            _intersections[x, y].RemoveIntersection();
            _intersections[x, y] = null;
        }
    }
}
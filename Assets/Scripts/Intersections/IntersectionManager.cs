using System;
using System.Collections.Generic;
using UnityEngine;

namespace Intersections
{
    public class IntersectionManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;

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


        public void CreateIntersection(int x, int y)
        {
            if (_intersections[x, y] is not null) return;
            var worldPosition = gridManager.GridToWorld(x, y);
            var newIntersection = new Intersection(x, y, worldPosition);
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
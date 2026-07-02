using System;
using System.Collections.Generic;
using Needs;
using UnityEngine;

namespace Intersections
{
    public class Intersection
    {
        private int _x;
        private int _y;
        private Vector3Int _position;
        
        private Queue<PathMover> _agents = new();

        public Intersection(int x, int y, Vector3Int worldPosition)
        {
            _x = x;
            _y = y;
            _position = worldPosition;
        }

        public void Process()
        {
            if (_agents.Count == 0) return; // nobody waiting at intersection
            // todo raycast, dequeue, tell to go
        }

        public void AddToQueue(PathMover agentToAdd)
        {
            _agents.Enqueue(agentToAdd);
            // todo tell to stop
        }
        
        public void RemoveIntersection()
        {
            foreach (var pathMover in _agents)
            {
                // todo tell to go
            }
        }
    }
}
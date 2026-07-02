using System;
using System.Collections.Generic;
using Needs;
using UnityEngine;

namespace Intersections
{
    public class Intersection
    {
        private readonly Node _position;
        private PathMover _lastSent;
        private readonly Queue<PathMover> _agents = new();

        public Intersection(Node position)
        {
            _position = position;
        }

        public void Process()
        {
            if (_lastSent is not null)
            {
                if (_lastSent.NextPosition != _position) // if the next position is no longer the junction
                {
                    _lastSent.MovingInJunction = false;
                    _lastSent = null;
                }
            }

            if (_lastSent is not null) return;
            if (_agents.Count == 0) return; // nobody waiting at intersection
            // if nothing is currently in the junction, send the next car that is waiting through
            _lastSent = _agents.Dequeue();
            _lastSent.Go();

        }

        public void AddToQueue(PathMover agentToAdd)
        {
            _agents.Enqueue(agentToAdd);
            if (agentToAdd != _lastSent)
                agentToAdd.Stop();
        }
        
        public void RemoveIntersection()
        {
            foreach (var pathMover in _agents)
            {
                pathMover.Go();
            }
        }
    }
}
using System.Collections.Generic;
using Agents;
using UnityEngine;

namespace Intersections
{
    public class Intersection
    {
        private readonly Vector3 _position;
        private readonly Node _node;
        private PathMover _lastSent;
        private readonly Queue<PathMover> _agents = new();

        public Intersection(Vector3 position, Node node)
        {
            _position = position;
            _node = node;
        }

        public void Process()
        {
            foreach (var pathMover in _agents)
            {
                Debug.DrawLine(
                    new Vector3(pathMover.WorldPosition.x, pathMover.WorldPosition.y + 1f,
                        pathMover.WorldPosition.z), pathMover.WorldPosition, Color.purple);
            }
            
            if (_lastSent is not null)
                Debug.DrawLine(new Vector3(_lastSent.WorldPosition.x, _lastSent.WorldPosition.y + 1f,
                    _lastSent.WorldPosition.z), _lastSent.WorldPosition, Color.orange);
            
            if (_lastSent is not null)
            {
                Debug.DrawLine(new Vector3(_position.x, _position.y + 1f, _position.z), _position, Color.blue);
                if (_lastSent.NextPosition != _node) // if the next position is no longer the junction
                {
                    _lastSent.MovingInJunction = false;
                    _lastSent = null;
                }
            }

            if (_lastSent is not null) return;
            Debug.DrawLine(new Vector3(_position.x, _position.y + 1f, _position.z), _position, Color.red);
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
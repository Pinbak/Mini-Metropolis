using System;
using System.Collections.Generic;
using Needs;
using UnityEngine;

namespace Intersections
{
    public class Intersection
    {
        private Node _node;
        private Vector3Int _position;
        private readonly Vector3 _rayCastPosition;
        private readonly LayerMask _rayCastLayer;

        private PathMover _lastSent;
        private readonly Queue<PathMover> _agents = new();

        public Intersection(Node node, Vector3Int worldPosition, LayerMask agentLayer)
        {
            _node = node;
            _position = worldPosition;
            _rayCastPosition = new Vector3(worldPosition.x, worldPosition.y + 1f, worldPosition.z);
            _rayCastLayer = agentLayer;
        }

        public void Process()
        {
            foreach (var pathMover in _agents)
            {
                Debug.DrawLine(
                    new Vector3(pathMover._agent.transform.position.x, pathMover._agent.transform.position.y + 1f,
                        pathMover._agent.transform.position.z), pathMover._agent.transform.position, Color.purple);
            }
            
            if (_lastSent is not null)
                Debug.DrawLine(new Vector3(_lastSent._agent.transform.position.x, _lastSent._agent.transform.position.y + 1f,
                    _lastSent._agent.transform.position.z), _lastSent._agent.transform.position, Color.orange);
            
            if (_lastSent is not null)
            {
                Debug.DrawLine(_rayCastPosition, _position, Color.blue);
                if (_lastSent.NextPosition != _node && _lastSent.CurrentPosition != _node)
                {
                    _lastSent.MovingInJunction = false;
                    _lastSent = null;
                    
                }
            }

            if (_lastSent is not null) return;
            Debug.DrawLine(_rayCastPosition, _position, Color.red);
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
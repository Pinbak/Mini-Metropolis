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
        private readonly Vector3 _rayCastPosition;
        private readonly LayerMask _rayCastLayer; 
        
        private readonly Queue<PathMover> _agents = new();

        public Intersection(int x, int y, Vector3Int worldPosition, LayerMask agentLayer)
        {
            _x = x;
            _y = y;
            _position = worldPosition;
            _rayCastPosition = new Vector3(worldPosition.x, worldPosition.y + 1f, worldPosition.z);
            _rayCastLayer = agentLayer;
        }

        public void Process()
        {
            if (_agents.Count == 0) return; // nobody waiting at intersection
            // if nothing is currently in the junction, send the next car that is waiting through.
            if (!Physics.Raycast(_rayCastPosition, Vector3.down, out _, 1f, _rayCastLayer))
            {
                Debug.DrawLine(_rayCastPosition, _rayCastPosition + Vector3.down, Color.blue);
                var nextInLine = _agents.Dequeue();
                nextInLine.Go();
            }
            else
            {
                Debug.DrawLine(_rayCastPosition, _rayCastPosition + Vector3.down, Color.red);
            }
            
        }

        public void AddToQueue(PathMover agentToAdd)
        {
            _agents.Enqueue(agentToAdd);
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
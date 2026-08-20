using System.Collections.Generic;
using Agents;
using UnityEngine;

namespace Junctions
{
    /// <summary>
    ///     A junction is defined as any road cell in the <see cref="Grid"/> that has 3 or more neighbours.
    /// </summary>
    public class Junction
    {
        private readonly Vector3 _position;
        private readonly Node _node;
        private PathMover _lastSent;
        private readonly Queue<PathMover> _agents = new();

        public Junction(Vector3 position, Node node)
        {
            _position = position;
            _node = node;
        }
        
        /// <summary>
        ///     To be called every tick. Caclulates what car should go next and when.
        /// </summary>
        public void Process()
        {
            foreach (var pathMover in _agents)
            {
                if (pathMover is { AgentExists: false }) return;
                Debug.DrawLine(
                    new Vector3(pathMover.WorldPosition.x, pathMover.WorldPosition.y + 1f,
                        pathMover.WorldPosition.z), pathMover.WorldPosition, Color.purple);
            }
            
            if (_lastSent is { AgentExists: true })
                Debug.DrawLine(new Vector3(_lastSent.WorldPosition.x, _lastSent.WorldPosition.y + 1f,
                    _lastSent.WorldPosition.z), _lastSent.WorldPosition, Color.orange);
            
            if (_lastSent is { AgentExists: true })
            {
                Debug.DrawLine(new Vector3(_position.x, _position.y + 1f, _position.z), _position, Color.blue);
                if (_lastSent.NextPosition != _node) // if the next position is no longer the junction
                {
                    _lastSent = null;
                }
            }

            if (_lastSent is { AgentExists: true }) return; // if the agent has been removed since, we can continue
            Debug.DrawLine(new Vector3(_position.x, _position.y + 1f, _position.z), _position, Color.red);
            if (_agents.Count == 0) return; // nobody waiting at intersection
            // if nothing is currently in the junction, send the next car that is waiting through
            _lastSent = _agents.Dequeue();
            _lastSent?.Go();
        }

        /// <summary>
        ///     Add the agent to this junction's queue.
        /// </summary>
        public void AddToQueue(PathMover agentToAdd)
        {
            _agents.Enqueue(agentToAdd);
            if (agentToAdd != _lastSent)
                agentToAdd.Stop();
        }
        
        /// <summary>
        ///     When deleting this junction, this has to be called, as the agents that are currently queueing need to
        ///     be told they can move.
        /// </summary>
        public void RemoveJunction()
        {
            foreach (var pathMover in _agents)
            {
                pathMover.Go();
            }
        }
    }
}
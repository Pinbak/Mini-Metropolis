using System;
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
        private readonly Node _thisNode;
        private PathMover _lastSent;
        private readonly List<PathMover> _agents = new();

        public Junction(Vector3 position, Node thisNode)
        {
            _position = position;
            _thisNode = thisNode;
        }
        
        /// <summary>
        ///     To be called every tick. Calculates what car should go next and when.
        /// </summary>
        public void Process()
        {
            if(_lastSent is not null)
            {
                if (_lastSent.NextPosition != _thisNode) // if the next position is no longer the junction
                {
                    _lastSent = null;
                }
            }

            if (_agents.Count == 0) return; // nobody waiting at junction
            // if nothing is currently in the junction, send the next car that is waiting through
            if (_lastSent is not null) return;
            _lastSent = _agents[0];
            _agents.Remove(_lastSent);
            _lastSent?.Go();
        }

        /// <summary>
        ///     Remove an agent prematurely from the queue
        /// </summary>
        public void RemoveAgentFromQueue(PathMover agent)
        {
            _agents.Remove(agent);
            if (_lastSent == agent)
                _lastSent = null;
        }

        /// <summary>
        ///     Add the agent to this junction's queue.
        /// </summary>
        public void AddToQueue(PathMover agentToAdd)
        {
            _agents.Add(agentToAdd);
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
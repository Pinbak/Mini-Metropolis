using System.Collections.Generic;
using Agents;
using UnityEngine;

namespace Junctions
{
    /// <summary>
    ///     Manages all junctions. A junction is defined as any road cell in the <see cref="Grid"/> that has 3 or
    ///     more neighbours.
    /// </summary>
    public class JunctionManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;

        private Junction[,] _junctions;
        private readonly List<Junction> _activeJunctions = new();

        private void Start()
        {
            // all the junctions that exist in the city
            _junctions = new Junction[gridManager.Width, gridManager.Height];
        }

        private void Update()
        {
            foreach (var activeJunction in _activeJunctions)
            {
                activeJunction.Process();
            }
        }
        
        /// <summary>
        ///     Add this agent to a junction queue
        /// </summary>
        public Junction AddToJunctionQueue(PathMover agentToAdd, Node junctionToAddTo)
        {
            var junction = _junctions[junctionToAddTo.X, junctionToAddTo.Y];
            junction.AddToQueue(agentToAdd);
            return junction;
        }
        
        /// <summary>
        ///     Checks if the node is a junction or not. Returns true if it is
        /// </summary>
        public bool IsJunction(Node node)
        {
            return _junctions[node.X, node.Y] is not null;
        }
        
        /// <summary>
        ///     Define a junction at this node
        /// </summary>
        public void CreateJunction(Node node)
        {
            var x = node.X;
            var y = node.Y;
            if (_junctions[x, y] is not null) return;
            var newJunction = new Junction(gridManager.GridToWorld(node.X, node.Y), node);
            _junctions[x, y] = newJunction;
            _activeJunctions.Add(newJunction);
        }
        
        /// <summary>
        ///     Remove the junction at this node
        /// </summary>
        public void RemoveJunction(int x, int y)
        {
            if (_junctions[x, y] is null) return;
            var junction = _junctions[x, y];
            _activeJunctions.Remove(junction);
            _junctions[x, y].RemoveJunction();
            _junctions[x, y] = null;
        }
    }
}
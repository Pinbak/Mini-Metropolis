using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents
{
    /// <summary>
    ///     Implementation of A*
    /// </summary>
    public class Pathfinding
    {
        public Node[] Path { get; private set; } = { };
        public bool ValidPathExists { get; private set; }
        
        private float Heuristic(Node current, Node goal)
        {
            // Manhattan distance
            return Mathf.Abs(current.X - goal.X) + Mathf.Abs(current.Y - goal.Y);
        }

        private Node[] ReconstructPath(Node start, Node current, Dictionary<Node, Node> cameFrom)
        {
            // reverse engineer the cameFrom tree to get a path stored as an array of nodes to travel to
            var path = new List<Node>();
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Add(start);
            path.Reverse();
            return path.ToArray();
        }

        public void GeneratePath(Node start, Node goal)
        {
            Path = FindPath(start, goal);
            ValidPathExists = Path.Length > 0;
        }

        private Node[] FindPath(Node start, Node goal)
        {
            // A* is defined as f(n) = g(n) + h(n)
            // g(n) is the total cost of transitions, which in our case is 1 per transition
            // h(n) is the heuristic which is the measured by the remaining Manhattan distance to the goal position
            
            var open = new Dictionary<Node, float> { [start] = 0f }; // items waiting to be expanded with their corresponding f(n)
            var closed = new List<Node>(); // items that have already been explored
            var cameFrom = new Dictionary<Node, Node>();
            var costSoFar = new Dictionary<Node, float> { [start] = 0f };
            var c = 0;
            while (open.Count != 0)
            {
                c++;
                var current = open.OrderBy(node => node.Value).First();
                var currentNode = current.Key;
                if (currentNode == goal)
                {
                    // once a path has been found, reverse engineer to make a usuable path and then return
                    return ReconstructPath(start, currentNode, cameFrom);
                }

                open.Remove(currentNode);
                closed.Add(currentNode);
                
                // traverse the tree using A* to find suitable next nodes
                foreach (var neighbour in currentNode.Neighbours)
                {
                    if (closed.Contains(neighbour)) continue;
                    var gScore = costSoFar[currentNode] + 1;
                    costSoFar[neighbour] = gScore;
                    var hScore = Heuristic(neighbour, goal);
                    var fScore = gScore + hScore;
                    open[neighbour] = fScore;
                    cameFrom[neighbour] = currentNode;
                    
                }

                // failsafe to prevent a memory exception from the recursion. Note may shut down huge cities, so should be increased
                if (c > 1000)
                {
                    return new Node[]{};
                }
            }

            // if no path was found (which is totally valid, as the road network may be incomplete) this should be expected and not treated as an error
            return new Node[]{};
        }
    }
}
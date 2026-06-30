using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needs
{
    public class Pathfinding
    {
        public List<Node> Path { get; private set; } = new();
        public bool ValidPathExists { get; private set; }
        
        private Grid _grid;
        
        public Pathfinding(Grid graph)
        {
            _grid = graph;
        }

        private float Heuristic(Node current, Node goal)
        {
            // Manhattan distance
            return Mathf.Abs(current.X - goal.X) + Mathf.Abs(current.Y - goal.Y);
        }

        private List<Node> ReconstructPath(Node start, Node current, Dictionary<Node, Node> cameFrom)
        {
            var path = new List<Node> { current };
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
            return path;
        }

        public void GeneratePath(Node start, Node goal)
        {
            Path = FindPath(start, goal);
            ValidPathExists = Path.Count > 0;
        }

        private List<Node> FindPath(Node start, Node goal)
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
                    Debug.Log("Found path!");
                    return ReconstructPath(start, currentNode, cameFrom);
                }

                open.Remove(currentNode);
                closed.Add(currentNode);
                
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

                if (c > 500)
                {
                    Debug.Log("Got stuck");
                    return new List<Node>();
                }
            }

            Debug.Log("Could not find a path!");
            return new List<Node>();
        }
    }
}
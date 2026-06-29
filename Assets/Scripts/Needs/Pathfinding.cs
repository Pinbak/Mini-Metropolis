using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needs
{
    public class Pathfinding
    {
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

        public List<Node> FindPath(Node start, Node goal)
        {
            var open = new List<Node> { start };
            var cameFrom = new Dictionary<Node, Node>();

            var gs = new Dictionary<Node, float> { [start] = 0f };
            var fs = new Dictionary<Node, float> { [start] = Heuristic(start, goal) };
            var c = 0;

            while (open.Count != 0)
            {
                c++;
                var current = fs.OrderBy(node => node.Value).First(); // todo inefficient
                if (current.Key == goal)
                {
                    Debug.Log("Found path");
                    return ReconstructPath(start, current.Key, cameFrom);
                }

                open.Remove(current.Key);
                foreach (var neighbour in current.Key.Neighbours)
                {
                    var newCost = gs[current.Key] + 10; // todo weight of the edge is arbitrarily 10 for now
                    if (gs.ContainsKey(neighbour) && !(newCost < gs[neighbour])) continue;
                    cameFrom[neighbour] = current.Key;
                    gs[neighbour] = newCost;
                    fs[neighbour] = newCost + Heuristic(neighbour, goal);
                    if (!open.Contains(neighbour)) open.Add(neighbour);
                }

                if (c > 500)
                {
                    Debug.Log("Exceeded maximum tries");
                    return new List<Node>();
                }
            }

            Debug.Log("Cannot find path");
            return new List<Node>();
        }
    }
}
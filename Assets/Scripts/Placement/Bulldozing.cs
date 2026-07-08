using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    public class Bulldozing : IPlacementState
    {
        private PlacementManager _context;
        
        public Bulldozing(PlacementManager context)
        {
            _context = context;
        }
        
        public void MouseDown(Vector3 position)
        {
            RemoveNode(position);
        }

        public void MouseRelease()
        {
        }

        public void MouseClick(Vector3 position)
        {
        }

        public void KeyboardPress(KeyboardKeys key)
        {
        }
        
        private void RemoveNode(Vector3 position)
        {
            var intPosition = new Vector3Int(Mathf.RoundToInt(position.x), 0, Mathf.RoundToInt(position.z));
            if (!_context.IsPositionInBound(intPosition)) return;
            var gridPosition = _context.GridManager.WorldToGrid(intPosition);
            var nodeToRemove = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
            if (nodeToRemove.Type is not NodeType.Road) return;

            var toRemove = new List<Node>{nodeToRemove};
            var toUpdate = new List<(int, int)> {(nodeToRemove.X, nodeToRemove.Y)};
            foreach (var neighbour in nodeToRemove.Neighbours)
            {
                // have to delete dependent nodes
                if (neighbour.Neighbours.Count == 1)
                {
                    toRemove.Add(neighbour);
                    toUpdate.Add((neighbour.X, neighbour.Y));
                }
            }

            foreach (var node in toRemove)
            {
                foreach (var neighbour in node.Neighbours)
                {
                    neighbour.Neighbours.Remove(node);
                    toUpdate.Add((neighbour.X, neighbour.Y));
                
                    if (neighbour.Neighbours.Count < 3)
                        _context.IntersectionManager.RemoveIntersection(neighbour.X, neighbour.Y);
                }

                node.Neighbours = new List<Node>();
                node.Type = NodeType.Empty;
                _context.IntersectionManager.RemoveIntersection(node.X, node.Y);
            }

            var chunksToRefresh = _context.GridManager.GetUniqueChunksFromPositions(toUpdate);
            foreach (var (chunkX, chunkY) in chunksToRefresh)
            {
                _context.GridManager.BuildChunk(chunkX, chunkY);
            }
        
        }
    }
}
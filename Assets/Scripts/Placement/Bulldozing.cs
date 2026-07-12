using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    public class Bulldozing : IPlacementState
    {
        private readonly PlacementManager _context;
        
        public Bulldozing(PlacementManager context)
        {
            _context = context;
        }
        
        public void MouseDown(Vector3 position)
        {
            var gridPosition = _context.GridManager.WorldToGrid(position);
            if (!_context.IsPositionInBound(gridPosition)) return;
            var node = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
            if (node.Type is NodeType.Road) RemoveNode(node);
            else if (node.Type is NodeType.Building or NodeType.Parking) RemoveBuilding(node);
        }

        public void MouseRelease() { }

        public void MouseClick(Vector3 position) { }

        public void KeyboardPress(KeyboardKeys key) { }

        public void MouseMove(Vector3 position) { }

        private void RemoveBuilding(Node node)
        {
            var buildingToRemove = _context.BuildingManager.AllBuildings[node.X, node.Y];
            if (buildingToRemove is null)
            {
                var zoneToRemove = _context.BuildingManager.AllZones[node.X, node.Y];
                _context.BuildingManager.RemoveZone(zoneToRemove);
            }
            else
            {
                _context.BuildingManager.RemoveBuilding(buildingToRemove);
            }
        }

        private void RemoveNode(Node nodeToRemove)
        {
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
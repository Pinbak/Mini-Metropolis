using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    public class BuildingRoad : IPlacementState
    {
        private readonly PlacementManager _context;
        private Vector3Int _startingPosition;
        private Vector3Int _lastSuccessfulPosition;
        private List<(int x, int y)> _validNeighbourNodes = new();
        private bool _mouseDown;
        private Vector2Int _previousPosition;

        public BuildingRoad(PlacementManager context)
        {
            _context = context;
        }

        public void EnterState()
        {
            _previousPosition = Vector2Int.zero;
        }

        public void ExitState()
        {
            _context.PlacementIndicator.RemoveMesh();
        }
        
        public void MouseDown(Vector3 position)
        {
            CheckPlacingRoad(position);
        }

        public void MouseRelease()
        {
            _mouseDown = false;
            _previousPosition = Vector2Int.zero;
        }

        public void MouseClick(Vector3 position)
        {
            _mouseDown = true;
            StartRoadPlacement(Vector3Int.RoundToInt(position));
        }

        public void KeyboardPress(KeyboardKeys key)
        {
        }

        public void MouseMove(Vector3 position)
        {
            if (_mouseDown) return;
            var nodePosition = _context.GridManager.WorldToGrid(position);
            if (_previousPosition == nodePosition) return;
            _previousPosition = nodePosition;
            
            var gridPosition = new Vector3(
                Mathf.RoundToInt(position.x), 0f, Mathf.RoundToInt(position.z)
            );
            _context.PlacementIndicator.DrawCircleAtPosition(gridPosition);
        }

        private void CheckPlacingRoad(Vector3 position)
        {
            _context.PlacementIndicator.DrawLineFromToPosition(position);
            var distance = Vector3.Distance(_startingPosition, position);
            if (!(distance > 1)) return;
            
            var direction = position - _startingPosition;
            direction.Normalize();
            var targetPosition = _startingPosition + new Vector3Int(
                Mathf.RoundToInt(direction.x), 0, Mathf.RoundToInt(direction.z));

            var gridTargetPosition = _context.GridManager.WorldToGrid(targetPosition);

            foreach (var validNeighbourNode in _validNeighbourNodes)
            {
                if (validNeighbourNode != (gridTargetPosition.x, gridTargetPosition.y)) continue;
                _lastSuccessfulPosition = targetPosition;
                EndRoadPlacement(gridTargetPosition);
                return;
            }
        }
        
        public void StartRoadPlacement(Vector3Int position)
        {
            if (!_context.IsPositionInBound(position)) return;
            if (!_context.IsPositionFreeOrRoad(position)) return;
            
            var gridPosition = _context.GridManager.WorldToGrid(position);
            _validNeighbourNodes = _context.GridManager.Grid.GetAdjacentCells(gridPosition.x, gridPosition.y);
            RemoveIllegalPlacements(position); // removes the ability to cross an existing road
            PlaceStartingNode(position);
        }
        
        private void EndRoadPlacement(Vector2Int endGridPosition)
        {
            // only gets called when the final placement is valid
            var startGridPosition =
                _context.GridManager.WorldToGrid(new Vector3Int(_startingPosition.x, _startingPosition.y, _startingPosition.z));

            var startNode = _context.GridManager.Grid[startGridPosition.x, startGridPosition.y];
            var endNode = _context.GridManager.Grid[endGridPosition.x, endGridPosition.y];

            // change to road if not already
            if (startNode.Type is NodeType.Empty) startNode.Type = NodeType.Road;
            if (endNode.Type is NodeType.Empty) endNode.Type = NodeType.Road;
        
            // add the neighbours for the connection
            startNode.Neighbours.Add(endNode);
            endNode.Neighbours.Add(startNode);
        
            _context.GridManager.BuildRoadMesh(startGridPosition.x, startGridPosition.y);
            _context.GridManager.BuildRoadMesh(endGridPosition.x, endGridPosition.y);

            if (startNode.Neighbours.Count > 2)
                _context.IntersectionManager.CreateIntersection(startNode);
            if (endNode.Neighbours.Count > 2)
                _context.IntersectionManager.CreateIntersection(endNode);
        
            // RemovePlanning();
            StartRoadPlacement(_lastSuccessfulPosition);
        }
        
        private void PlaceStartingNode(Vector3Int position)
        {
            _startingPosition = position;
            _lastSuccessfulPosition = position;
            _context.PlacementIndicator.UpdateStartPosition(position);
        }

        private void RemoveIllegalPlacements(Vector3Int position)
        {
            var gridPosition = _context.GridManager.WorldToGrid(position);
            var diagonals = _context.GridManager.Grid.GetDiagonalCells(gridPosition.x, gridPosition.y);
            var illegalPlacements = new List<(int x, int y)>();

            foreach (var (dX, dY) in diagonals)
            {
                var sharedNeighbours = _context.GridManager.Grid.GetSharedNeighbours(gridPosition.x, gridPosition.y, dX, dY);
                // check if any of the shared neighbours are connected
                foreach (var sharedNeighbour in sharedNeighbours)
                foreach (var sharedNeighbour2 in sharedNeighbours)
                {
                    if (sharedNeighbour == sharedNeighbour2) continue;
                    var sharedNeighbourNode = _context.GridManager.Grid[sharedNeighbour.x, sharedNeighbour.y];
                    var sharedNeighbour2Node = _context.GridManager.Grid[sharedNeighbour2.x, sharedNeighbour2.y];
                    if (sharedNeighbourNode.Neighbours.Contains(sharedNeighbour2Node))
                        illegalPlacements.Add((dX, dY));
                }
            }

            foreach (var illegalPlacement in illegalPlacements)
            {
                _validNeighbourNodes.Remove(illegalPlacement);
            }
        
        }
    }
}
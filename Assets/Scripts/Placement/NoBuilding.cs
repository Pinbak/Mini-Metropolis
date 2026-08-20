using Buildings;
using UnityEngine;

namespace Placement
{
    /// <summary>
    ///     If the player is not placing anything. Used to show information to the player via hovering over buildings.
    /// </summary>
    public class NoBuilding : IPlacementState
    {
        private readonly PlacementManager _context;
        private Vector2Int _previousPosition;
        private Building _buildingCurrentlyShown;
        private const float UIOffset = .5f;

        public NoBuilding(PlacementManager context)
        {
            _context = context;
        }

        public void EnterState()
        {
            _previousPosition = Vector2Int.zero;
        }

        public void ExitState()
        {
            if (_buildingCurrentlyShown is null) return;
            _context.BuildingInformation.Hide();
        }

        public void MouseDown(Vector3 position) { }

        public void MouseRelease() { }

        public void MouseClick(Vector3 position)
        {
            if (_context.GridManager.IsWorldPositionOutsideOfGrid(position)) return;
            var nodePosition = _context.GridManager.WorldToGrid(position);
            if (_previousPosition == nodePosition) return;
            _previousPosition = nodePosition;
            var node = _context.GridManager.Grid[nodePosition.x, nodePosition.y];
            
            // if no building is here, remove the information UI
            if (node.Type is NodeType.Empty or NodeType.Road)
            {
                if (_buildingCurrentlyShown is null) return;
                _context.BuildingInformation.Hide();
                _buildingCurrentlyShown = null;
                return;
            }
            
            // if a building is here, get it and display the information
            var buildingHere = _context.BuildingManager.AllBuildings[nodePosition.x, nodePosition.y];
            if (buildingHere == _buildingCurrentlyShown) return; // currently already showing it
            if (buildingHere is null) return;
            // show information about the building
            _buildingCurrentlyShown = buildingHere;
            _context.BuildingInformation.Hide();
            var buildingTop = buildingHere.Top;
            _context.BuildingInformation.Show(buildingHere, buildingTop.x, buildingTop.z + UIOffset);
        }

        public void KeyboardPress(KeyboardKeys key) { }

        public void MouseMove(Vector3 position) { }
    }
}
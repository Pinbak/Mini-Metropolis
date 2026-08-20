using Buildings;
using UnityEngine;

namespace Placement
{
    /// <summary>
    ///     Placing a building state.
    /// </summary>
    public class BuildingZone : IPlacementState
    {
        public Building Places { get; private set; } // the building that is placed
        private readonly PlacementManager _context;
        private const float IndicatorGroundClearance = .1f;
        private Color _zoneColour;
        private Color _invalidPlacementColour;
        private Building _previewBuilding;
        private Building _previewBuildingPrefab;
        private Zone _previewZone;
        private int _cost;
        private Quaternion _rotation = Quaternion.Euler(0f, 0f, 0f);

        /// <summary>
        ///     Change the building that this mode will place
        /// </summary>
        public void ChangePlacementBuilding(Building building)
        {
            Places = building;
            _cost = Places.Cost;
            _zoneColour = _context.ColourSampler.GetColourByBuildingType(building.Type);
            _invalidPlacementColour = _context.ColourSampler.GetInvalidColour();
            _previewBuildingPrefab = building;
        }
        
        public BuildingZone(PlacementManager context)
        {
            _context = context;
        }
        
        public void EnterState()
        {
            // create a preview to show the player. This preview object will cease to exist, once the state has changed
            _previewBuilding = Object.Instantiate(_previewBuildingPrefab, Vector3.zero, _rotation,
                _context.transform);
            _previewZone = Object.Instantiate(_context.ZonePrefab, Vector3.zero, _rotation,
                _context.transform);
            _previewZone.Init(_context.BuildingManager, _previewBuildingPrefab, _previewBuilding.Layout,
                _previewBuilding.ParkingSpaces);
        }

        public void ExitState()
        {
            // remove the preview object, as it was only for this state
            if (_previewZone is not null) Object.Destroy(_previewZone.gameObject);
            if (_previewBuilding is not null) Object.Destroy(_previewBuilding.gameObject);
        }

        private void Rotate(float angle)
        {
            // rotates the preview, while keeping track of the current rotation, as that will be needed for the
            // instantiation of the building if placed
            if (_previewBuilding is null || _previewZone is null) return;
            _previewBuilding.transform.Rotate(Vector3.up, angle);
            _previewZone.transform.Rotate(Vector3.up, angle);
            _rotation = _previewZone.transform.rotation;
        }
        
        public void MouseDown(Vector3 position) { }

        public void MouseRelease() { }

        public void MouseClick(Vector3 position)
        {
            if (_cost > _context.BuildingManager.Balance) return; // can't afford it
            if (Places.IsGrowable)
                CreateZone(_context.GridManager.WorldToNode(position), Places);
            else
                CreateBuilding(_context.GridManager.WorldToNode(position), Places);
        }

        public void KeyboardPress(KeyboardKeys key)
        {
            if (key is KeyboardKeys.R) Rotate(90f); // rotate with the "R" key
        }

        public void MouseMove(Vector3 position)
        {
            // updates the preview buildings position to where the mouse is, given it is not off the grid
            if (_context.GridManager.IsWorldPositionOutsideOfGrid(position)) return;
            UpdateIndicator(position);
        }

        private void UpdateIndicator(Vector3 mousePosition)
        {
            // update the colour to show if the placement is valid, given its in bounds and the player can afford it
            _previewZone.SetOutlineColour(
                IsSpaceAvailable(_previewZone.Layout) && _cost <= _context.BuildingManager.Balance
                    ? _zoneColour
                    : _invalidPlacementColour);

            var gridPosition = new Vector3(
                Mathf.RoundToInt(mousePosition.x),
                IndicatorGroundClearance,
                Mathf.RoundToInt(mousePosition.z)
            );
            // update the preview's transforms
            if (_previewBuilding is not null)
                _previewBuilding.transform.position = gridPosition - new Vector3(0f, IndicatorGroundClearance, 0f);
            if (_previewZone is not null) _previewZone.transform.position = gridPosition;
        }

        private void CreateBuilding(Node position, Building type)
        {
            if (!IsSpaceAvailable(_previewBuilding.Layout)) return;
            var building = Object.Instantiate(type, _context.GridManager.NodeToWorld(position), _rotation,
                _context.BuildingManager.transform);
            building.Init(_context.BuildingManager);
            _context.BuildingManager.Balance -= _cost;
            // place the building, which updates the grid to reflect the changes
            PlaceBuilding(building);
        }

        private void CreateZone(Node position, Building type)
        {
            if (!IsSpaceAvailable(_previewZone.Layout)) return;

            var newZone = Object.Instantiate(_context.ZonePrefab, _context.GridManager.NodeToWorld(position), _rotation,
                _context.BuildingManager.transform);
            newZone.Init(_context.BuildingManager, type, _previewBuilding.Layout, _previewBuilding.ParkingSpaces);
            _context.BuildingManager.Balance -= _cost;
            PlaceZone(newZone);

            switch (type.Type)
            {
                // keep track of zones
                case BuildingType.Residential:
                    _context.BuildingManager.ResidentialZones.Add(newZone);
                    break;
                case BuildingType.Commercial:
                    _context.BuildingManager.CommercialZones.Add(newZone);
                    break;
                case BuildingType.Industrial:
                    _context.BuildingManager.IndustrialZones.Add(newZone);
                    break;
            }
        }

        private bool IsSpaceAvailable(LayoutPosition[] layout)
        {
            // use the layout positions to cross-reference with the grid to see if nothing is there
            foreach (var layoutPosition in layout)
            {
                var gridPosition = _context.GridManager.WorldToGrid(layoutPosition.transform.position);
                if (gridPosition.x >= _context.GridManager.Width || gridPosition.x < 0) return false;
                if (gridPosition.y >= _context.GridManager.Height || gridPosition.y < 0) return false;
                var node = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
                if (node.Type is not NodeType.Empty) return false;
            }
            return true;
        }

        /// <summary>
        ///     Warning is destructive!
        /// </summary>
        private void PlaceZone(Zone zone)
        {
            // changes the grid to reflect the new zone
            foreach (var layoutPosition in zone.Layout)
            {
                var gridPosition = _context.GridManager.WorldToGrid(layoutPosition.transform.position);
                var node = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
                _context.BuildingManager.AllZones[gridPosition.x, gridPosition.y] = zone;
                node.Type = layoutPosition.Type;
            }
        }
        
        public void PlaceBuilding(Building building)
        {
            // changes the grid to reflect the new building
            foreach (var layoutPosition in building.Layout)
            {
                var gridPosition = _context.GridManager.WorldToGrid(layoutPosition.transform.position);
                var node = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
                _context.BuildingManager.AllBuildings[gridPosition.x, gridPosition.y] = building;
                node.Type = layoutPosition.Type;
            }
        }
    }
}
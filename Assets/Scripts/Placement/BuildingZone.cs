using Buildings;
using UnityEngine;

namespace Placement
{
    public class BuildingZone : IPlacementState
    {
        public Building Places { get; private set; }
        private readonly PlacementManager _context;
        private const float IndicatorGroundClearance = .1f;
        private const float IndicatorLerpSpeed = .1f;
        private Color _zoneColour;
        private Color _invalidPlacementColour;
        private Node _currentPosition;

        public void ChangePlacementBuilding(Building building)
        {
            Places = building;
            _zoneColour = _context.ColourSampler.GetColourByBuildingType(building.Type);
            _invalidPlacementColour = _context.ColourSampler.GetInvalidColour();
            var width = building.Width;
            var height = building.Height;
            
            _context.PlacementIndicator.startColor = _zoneColour;
            _context.PlacementIndicator.endColor = _zoneColour;
            const float offset = -0.5f;
            _context.PlacementIndicator.SetPosition(0, new Vector3(offset, IndicatorGroundClearance, offset));
            _context.PlacementIndicator.SetPosition(1, new Vector3(width + offset, IndicatorGroundClearance, offset));
            _context.PlacementIndicator.SetPosition(2, new Vector3(width + offset, IndicatorGroundClearance, height + offset));
            _context.PlacementIndicator.SetPosition(3, new Vector3(offset, IndicatorGroundClearance, height + offset));
            _context.PlacementIndicator.enabled = true;
        }
        
        public BuildingZone(PlacementManager context)
        {
            _context = context;
        }
        
        public void MouseDown(Vector3 position) { }

        public void MouseRelease() { }

        public void MouseClick(Vector3 position)
        {
            CreateZone(_context.GridManager.WorldToNode(position), Places);
        }

        public void KeyboardPress(KeyboardKeys key) { }

        public void MouseMove(Vector3 position)
        {
            if (_context.GridManager.IsWorldPositionOutsideOfGrid(position)) return;
            var currentNode = _context.GridManager.WorldToNode(position);
            _currentPosition ??= currentNode;
            if (currentNode == _currentPosition) return; // haven't moved the mouse
            _currentPosition = currentNode;
            if (IsSpaceAvailable(Places.Width, Places.Height, currentNode))
            {
                _context.PlacementIndicator.startColor = _zoneColour;
                _context.PlacementIndicator.endColor = _zoneColour;
            }
            else
            {
                _context.PlacementIndicator.startColor = _invalidPlacementColour;
                _context.PlacementIndicator.endColor = _invalidPlacementColour;
            }
            
            var gridPosition = new Vector3(
                Mathf.RoundToInt(position.x),
                IndicatorGroundClearance,
                Mathf.RoundToInt(position.z)
            );
            _context.PlacementIndicator.transform.position = gridPosition;
                // Vector3.Lerp(_context.PlacementIndicator.transform.position, gridPosition, IndicatorLerpSpeed);
        }
        
        private void CreateZone(Node position, Building type)
        {
            var width = type.Width;
            var height = type.Height;
            if (!IsSpaceAvailable(width, height, position)) return;

            // todo using object check that that's fine
            var newZone = Object.Instantiate(_context.ZonePrefab, _context.GridManager.NodeToWorld(position),
                Quaternion.identity, _context.BuildingManager.transform);
            newZone.Init(_context.BuildingManager, type);
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

        private bool IsSpaceAvailable(int width, int height, Node bottomLeft)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var gridPosition = new Vector2Int(bottomLeft.X + x, bottomLeft.Y + y);
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
        public static void PlaceBuilding(Building building, PlacementManager context)
        {
            for (var x = 0; x < building.Width; x++)
            for (var y = 0; y < building.Height; y++)
            {
                var gridPosition = new Vector2Int(building.BottomLeft.X + x, building.BottomLeft.Y + y);
                var node = context.GridManager.Grid[gridPosition.x, gridPosition.y];
                node.Type = building.Layout[x, y];
            }
        }

        private void PlaceZone(Zone zone)
        {
            for (var x = 0; x < zone.Width; x++)
            for (var y = 0; y < zone.Height; y++)
            {
                var gridPosition = new Vector2Int(zone.BottomLeft.X + x, zone.BottomLeft.Y + y);
                var node = _context.GridManager.Grid[gridPosition.x, gridPosition.y];
                if (node.Type is not NodeType.Empty) return;
                node.Type = zone.Layout[x, y];
            }
        }
    }
}
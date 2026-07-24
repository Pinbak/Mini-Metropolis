using Buildings;
using UnityEngine;

namespace Placement
{
    public class BuildingZone : IPlacementState
    {
        public Building Places { get; private set; }
        private readonly PlacementManager _context;
        private const float IndicatorGroundClearance = .1f;
        private Color _zoneColour;
        private Color _invalidPlacementColour;
        private Building _previewBuilding;
        private Building _previewBuildingPrefab;
        private Zone _previewZone;
        private int _cost;
        private Quaternion _rotation = Quaternion.Euler(0f, 0f, 0f);

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
            _previewBuilding = Object.Instantiate(_previewBuildingPrefab, Vector3.zero, _rotation,
                _context.transform);
            _previewZone = Object.Instantiate(_context.ZonePrefab, Vector3.zero, _rotation,
                _context.transform);
            _previewZone.Init(_context.BuildingManager, _previewBuildingPrefab, _previewBuilding.Layout,
                _previewBuilding.ParkingSpaces);
        }

        public void ExitState()
        {
            if (_previewZone is not null) Object.Destroy(_previewZone.gameObject);
            if (_previewBuilding is not null) Object.Destroy(_previewBuilding.gameObject);
        }

        private void Rotate(float angle)
        {
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
            if (key is KeyboardKeys.R) Rotate(90f);
        }

        public void MouseMove(Vector3 position)
        {
            if (_context.GridManager.IsWorldPositionOutsideOfGrid(position)) return;
            UpdateIndicator(position);
        }

        private void UpdateIndicator(Vector3 mousePosition)
        {
            _previewZone.SetOutlineColour(
                IsSpaceAvailable(_previewZone.Layout) && _cost <= _context.BuildingManager.Balance
                    ? _zoneColour
                    : _invalidPlacementColour);

            var gridPosition = new Vector3(
                Mathf.RoundToInt(mousePosition.x),
                IndicatorGroundClearance,
                Mathf.RoundToInt(mousePosition.z)
            );
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
        public void PlaceZone(Zone zone)
        {
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
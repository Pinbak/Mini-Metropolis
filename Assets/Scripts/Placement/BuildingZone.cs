using Needs.Buildings;
using UnityEngine;

namespace Placement
{
    public class BuildingZone : IPlacementState
    {
        public BuildingType Places { get; set; } = BuildingType.Residential;
        private readonly PlacementManager _context;
        
        public BuildingZone(PlacementManager context)
        {
            _context = context;
        }
        
        public void MouseDown(Vector3 position)
        {
        }

        public void MouseRelease()
        {
        }

        public void MouseClick(Vector3 position)
        {
            switch (Places)
            {
                case BuildingType.Residential:
                    CreateResidentialZone(_context.GridManager.WorldToNode(position));
                    break;
                case BuildingType.Industrial:
                    CreateIndustrialZone(_context.GridManager.WorldToNode(position));
                    break;
            }
        }

        public void KeyboardPress(KeyboardKeys key)
        {
        }
        
        public void CreateResidentialZone(Node position)
        {
            CreateZone(position, _context.ResidentialLowWealthPrefab);
        }
    
        public void CreateCommercialZone(Node position)
        {
            // CreateZone(position, );
        }

        public void CreateIndustrialZone(Node position)
        {
            CreateZone(position, _context.IndustrialLowWealthPrefab);
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
        
        public bool IsSpaceAvailable(int width, int height, Node bottomLeft)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var gridPosition = new Vector2Int(bottomLeft.X + x, bottomLeft.Y + y);
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
    
        public void PlaceZone(Zone zone)
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
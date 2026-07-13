using System.Collections.Generic;
using Placement;
using UnityEngine;

namespace Buildings
{
    public class Zone : MonoBehaviour
    {
        public Building Builds { get; private set; }
        
        [field:SerializeField] public int Width { get; set; }
        [field:SerializeField] public int Height { get; set; }
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private ColourSampler colourSampler;
        [SerializeField] private ArrowIndicator arrowIndicatorPrefab;
        private Color _color;
        private const float OffsetHeight = .1f;
        
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }
        
        public NodeType[,] Layout { get; private set; }
        
        private BuildingManager BuildingManager { get; set; }

        public void Init(BuildingManager buildingManager, Building builds)
        {
            BuildingManager = buildingManager;
            var position = transform.position;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            BottomLeft = bottomLeft;
            WorldPosition = buildingManager.GridManager.NodeToWorld(bottomLeft);
            Builds = builds;
            Width = builds.Width;
            Height = builds.Height;
            GenerateLayout();
            GenerateOutline();
            CreateArrowForParking();
        }

        private void GenerateOutline()
        {
            // runs once
            const float offset = -0.5f;

            _color = colourSampler.GetColourByBuildingType(Builds.Type);
            SetOutlineColour(_color);
            lineRenderer.SetPosition(0, new Vector3(offset, OffsetHeight, offset));
            lineRenderer.SetPosition(1, new Vector3(Width + offset, OffsetHeight, offset));
            lineRenderer.SetPosition(2, new Vector3(Width + offset, OffsetHeight, Height + offset));
            lineRenderer.SetPosition(3, new Vector3(offset, OffsetHeight, Height + offset));
        }

        public void SetOutlineColour(Color colour)
        {
            lineRenderer.startColor = colour;
            lineRenderer.endColor = colour;
        }

        private void GenerateLayout()
        {
            Layout = new NodeType[Width, Height];
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                Layout[x, y] = NodeType.Building;
            }
        }

        private void CreateArrowForParking()
        {
            var uniquePositions = new HashSet<(Vector3 position, Vector3 direction)>();
            foreach (var parkingSpace in Builds.ParkingSpaces)
            {
                var direction = parkingSpace.RoadConnection - parkingSpace.ParentPosition;
                uniquePositions.Add((parkingSpace.ParentPosition + transform.position, direction));
            }

            var offsetHeight = new Vector3(0f, OffsetHeight, 0f);
            foreach (var uniquePosition in uniquePositions)
            {
                var arrow = Instantiate(arrowIndicatorPrefab, uniquePosition.position + offsetHeight,
                    Quaternion.LookRotation(uniquePosition.direction) * Quaternion.Euler(90, 0, 0), transform);
                arrow.SetColour(_color);
            }
        }
        
        
    }
}
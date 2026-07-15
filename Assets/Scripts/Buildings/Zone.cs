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
        private readonly List<ArrowIndicator> _arrows = new();
        
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }
        
        public LayoutPosition[] Layout { get; set; }
        
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
            GenerateOutline();
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

        public void UpdateArrowParkingIndicators(ParkingSpace[] parkingSpaces)
        {
            foreach (var arrowIndicator in _arrows) Destroy(arrowIndicator.gameObject); // remove existing
            
            var uniquePositions = new HashSet<(Vector3 position, Vector3 direction)>();
            foreach (var parkingSpace in parkingSpaces)
            {
                var direction = parkingSpace.RoadConnection - parkingSpace.ParentPosition;
                uniquePositions.Add((parkingSpace.ParentPosition + transform.position, direction));
            }

            var offsetHeight = new Vector3(0f, OffsetHeight, 0f);
            foreach (var uniquePosition in uniquePositions)
            {
                var arrow = Instantiate(arrowIndicatorPrefab, uniquePosition.position + offsetHeight,
                    Quaternion.LookRotation(uniquePosition.direction) * Quaternion.Euler(90f, 0f, 0f), transform);
                arrow.SetColour(_color);
                _arrows.Add(arrow);
            }
        }
    }
}
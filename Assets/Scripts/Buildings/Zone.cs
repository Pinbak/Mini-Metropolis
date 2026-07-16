using System.Collections.Generic;
using System.Linq;
using Placement;
using UnityEngine;

namespace Buildings
{
    public class Zone : MonoBehaviour
    {
        public Building Builds { get; private set; }
        public LayoutPosition[] Layout { get; private set; }

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private ColourSampler colourSampler;
        [SerializeField] private ArrowIndicator arrowIndicatorPrefab;
        
        private Color _color;
        private const float OffsetHeight = .1f;
        private readonly List<ArrowIndicator> _arrows = new();
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }
        
        private BuildingManager BuildingManager { get; set; }

        public void Init(BuildingManager buildingManager, Building builds, Building preview)
        {
            BuildingManager = buildingManager;
            var position = transform.position;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            BottomLeft = bottomLeft;
            WorldPosition = buildingManager.GridManager.NodeToWorld(bottomLeft);
            Builds = builds;
            
            Layout = CopyLayout(preview.Layout);
            GenerateOutline();
            GenerateArrowParkingIndicators(preview.ParkingSpaces);
        }


        /// <summary>
        ///     Recreates the preview buildings layout, so that it can be used for placement and deletion
        /// </summary>
        private LayoutPosition[] CopyLayout(LayoutPosition[] layout) =>
            layout.Select(layoutPosition => Instantiate(layoutPosition, transform)).ToArray();

        private void GenerateOutline()
        {
            // runs once
            const float offset = -0.5f;

            var minX = Layout.Min(v => v.transform.localPosition.x);
            var maxX = Layout.Max(v => v.transform.localPosition.x);
            var minZ = Layout.Min(v => v.transform.localPosition.z);
            var maxZ = Layout.Max(v => v.transform.localPosition.z);
            var width = (int)(maxX - minX + 1);
            var height = (int)(maxZ - minZ + 1);

            _color = colourSampler.GetColourByBuildingType(Builds.Type);
            SetOutlineColour(_color);
            lineRenderer.SetPosition(0, new Vector3(offset, OffsetHeight, offset));
            lineRenderer.SetPosition(1, new Vector3(width + offset, OffsetHeight, offset));
            lineRenderer.SetPosition(2, new Vector3(width + offset, OffsetHeight, height + offset));
            lineRenderer.SetPosition(3, new Vector3(offset, OffsetHeight, height + offset));
        }

        public void SetOutlineColour(Color colour)
        {
            lineRenderer.startColor = colour;
            lineRenderer.endColor = colour;
        }

        private void GenerateArrowParkingIndicators(ParkingSpace[] parkingSpaces)
        {
            foreach (var arrowIndicator in _arrows) Destroy(arrowIndicator.gameObject); // remove existing
            
            var uniquePositions = new HashSet<(Vector3 position, Vector3 direction)>();
            foreach (var parkingSpace in parkingSpaces)
            {
                var direction = parkingSpace.RoadConnection - parkingSpace.ParentPosition;
                uniquePositions.Add((parkingSpace.ParentPosition, direction));
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
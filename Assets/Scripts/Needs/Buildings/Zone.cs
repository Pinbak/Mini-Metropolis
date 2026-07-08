using System;
using UnityEngine;

namespace Needs.Buildings
{
    public class Zone : MonoBehaviour
    {
        public Building Builds { get; private set; }
        
        [field:SerializeField] public int Width { get; set; }
        [field:SerializeField] public int Height { get; set; }
        [SerializeField] private LineRenderer lineRenderer;
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
        }

        public void GenerateOutline()
        {
            var offset = -0.5f;
            var height = .1f;
            lineRenderer.SetPosition(0, new Vector3(offset, height, offset));
            lineRenderer.SetPosition(1, new Vector3(Width + offset, height, offset));
            lineRenderer.SetPosition(2, new Vector3(Width + offset, height, Height + offset));
            lineRenderer.SetPosition(3, new Vector3(offset, height, Height + offset));
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
    }
}
using System;
using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Residential : MonoBehaviour
    {
        public BuildingInformation BuildingInformation { get; private set; }

        // the people that live here
        [SerializeField] private Commuter commuterPrefab;
        [SerializeField] private BuildingType type;
        [SerializeField] private ParkingSpace[] validParkingSpaces;
        private const int Width = 2;
        private const int Height = 1;

        private BuildingManager _buildingManager;
        private Commuter _commuter;

        private void Update()
        {
            
        }

        public void Init(BuildingManager buildingManager)
        {
            var position = transform.position;
            _buildingManager = buildingManager;
            var layout = new[,]
            {
                { NodeType.Parking },
                { NodeType.Building }
            };
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            BuildingInformation = new BuildingInformation(Width, Height, bottomLeft, position, layout);
            foreach (var parkingSpace in validParkingSpaces)
            {
                _commuter = Instantiate(commuterPrefab, parkingSpace.transform.position, Quaternion.identity,
                    transform);
                _commuter.Init(buildingManager, buildingManager.GridManager, buildingManager.IntersectionManager,
                    buildingManager.CarAcceleration,
                    buildingManager.GridManager.WorldToNode(parkingSpace.ParentPosition));
            }
            
        }

        private void OnDrawGizmos()
        {
            if (BuildingInformation is null) return;
            for (var x = 0; x < BuildingInformation.Width; x++)
            for (var y = 0; y < BuildingInformation.Height; y++)
            {
                var gridPosition = new Vector2Int(BuildingInformation.BottomLeft.X + x, BuildingInformation.BottomLeft.Y + y);
                var node = _buildingManager.GridManager.Grid[gridPosition.x, gridPosition.y];
                var worldPosition = _buildingManager.GridManager.NodeToWorld(node);
                Gizmos.color = Color.red;

                if (node.Type is NodeType.Parking)
                    Gizmos.color = Color.blue;
                
                Gizmos.DrawSphere(new Vector3(worldPosition.x, worldPosition.y + 1f, worldPosition.z), .1f);
            }
            
        }
    }
}
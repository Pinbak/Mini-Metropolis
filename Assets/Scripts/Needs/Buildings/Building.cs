using System.Collections.Generic;
using System.Linq;
using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Building : MonoBehaviour
    {
        [field:SerializeField] public int Width { get; set; }
        [field:SerializeField] public int Height { get; set; }
        [field:SerializeField] public ParkingSpace[] ParkingSpaces { get; set; }
        [field:SerializeField] public NodeType[,] Layout { get; set; }
        [SerializeField] private Agent[] supplies;
        [SerializeField] private Agent[] demands;

        public List<Agent> Supplies { get; set; } = new();
        
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }
        
        protected BuildingManager BuildingManager { get; private set; }

        public void Init(BuildingManager buildingManager, NodeType[,] layout)
        {
            var position = transform.position;
            BuildingManager = buildingManager;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            
            var gridManager = buildingManager.GridManager;
            BottomLeft = bottomLeft;
            WorldPosition = gridManager.NodeToWorld(bottomLeft);
            Layout = layout;

            for (var i = 0; i < supplies.Length; i++)
            {
                var agentPrefab = supplies[i];
                var agent = Instantiate(agentPrefab, ParkingSpaces[i].transform.position, Quaternion.identity, transform);
                agent.Init(this, buildingManager.TestIndustrial, buildingManager, ParkingSpaces[i]);
            }
        }
        
        public bool CheckParkingIsFree()
        {
            return ParkingSpaces.Any(parkingSpace => parkingSpace.IsBeingTaken);
        }

        public bool GetFreeParkingSpace(out ParkingSpace freeParkingSpace)
        {
            freeParkingSpace = null;
            foreach (var parkingSpace in ParkingSpaces)
            {
                if (parkingSpace.IsBeingTaken) continue;
                freeParkingSpace = parkingSpace;
                return true;
            }
            
            return false;
        }
        
        private void OnDrawGizmos()
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                var gridPosition = new Vector2Int(BottomLeft.X + x, BottomLeft.Y + y);
                var node = BuildingManager.GridManager.Grid[gridPosition.x, gridPosition.y];
                var worldPosition = BuildingManager.GridManager.NodeToWorld(node);
                Gizmos.color = Color.red;

                if (node.Type is NodeType.Parking)
                    Gizmos.color = Color.blue;
                
                Gizmos.DrawSphere(new Vector3(worldPosition.x, worldPosition.y + 1f, worldPosition.z), .1f);
            }
            
        }
    }
}
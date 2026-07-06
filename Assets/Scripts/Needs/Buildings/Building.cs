using System.Linq;
using UnityEngine;

namespace Needs.Buildings
{
    public class Building : MonoBehaviour
    {
        [field:SerializeField] public int Width { get; set; }
        [field:SerializeField] public int Height { get; set; }
        [field:SerializeField] public ParkingSpace[] ParkingSpaces { get; set; }
        public Node BottomLeft { get; set; }
        public Vector3 WorldPosition { get; set; }
        public NodeType[,] Layout { get; set; }

        protected BuildingManager BuildingManager { get; private set; }

        public void Init(BuildingManager buildingManager)
        {
            var position = transform.position;
            BuildingManager = buildingManager;
            var layout = new[,]
            {
                { NodeType.Parking, NodeType.Building },
                { NodeType.Building, NodeType.Building }
            };
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            
            var gridManager = buildingManager.GridManager;
            BottomLeft = bottomLeft;
            WorldPosition = gridManager.NodeToWorld(bottomLeft);
            Layout = layout;
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
    }
}
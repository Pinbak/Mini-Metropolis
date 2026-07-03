using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Needs.Buildings
{
    public class BuildingInformation
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Node BottomLeft { get; set; }
        
        private Vector3 _position;
        
        public NodeType[,] Layout { get; set; }
        public ParkingSpace[] ParkingSpaces { get; set; }

        private GridManager _gridManager;

        public BuildingInformation(GridManager gridManager, int width, int height, Node bottomLeft, NodeType[,] layout,
            ParkingSpace[] parkingSpaces)
        {
            _gridManager = gridManager;
            Width = width;
            Height = height;
            BottomLeft = bottomLeft;
            _position = gridManager.NodeToWorld(bottomLeft);
            Layout = layout;
            ParkingSpaces = parkingSpaces;
        }
        

        public bool CheckParkingIsFree()
        {
            return ParkingSpaces.Any(parkingSpace => parkingSpace.IsFree);
        }

        public ParkingSpace Park(PathMover agentToPark)
        {
            foreach (var parkingSpace in ParkingSpaces)
            {
                if (!parkingSpace.IsFree) continue;
                return parkingSpace;
            }

            return null;
        }
    }
}
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
        public Vector3 WorldPosition { get; set; }
        
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
            WorldPosition = gridManager.NodeToWorld(bottomLeft);
            Layout = layout;
            ParkingSpaces = parkingSpaces;
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
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

        public BuildingInformation(int width, int height, Node bottomLeft, Vector3 position, NodeType[,] layout)
        {
            Width = width;
            Height = height;
            BottomLeft = bottomLeft;
            _position = position;
            Layout = layout;
        }
    }
}
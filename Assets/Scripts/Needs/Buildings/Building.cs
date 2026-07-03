using UnityEngine;

namespace Needs.Buildings
{
    public class Building
    {
        private int _width;
        private int _height;
        private Node _node;
        private Vector3 _position;

        public Building(int width, int height, Node node, Vector3 position)
        {
            _width = width;
            _height = height;
            _node = node;
            _position = position;
        }
    }
}
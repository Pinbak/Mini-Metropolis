using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class Vector3Path
    {
        // Converts a path of nodes into a viable path of vector3 points to follow
        public List<Vector3> Path { get; private set; } = new();

        private readonly GridManager _gridManager;

        public Vector3Path(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        public void GeneratePath(List<Node> nodePath)
        {
            Path = new();
            foreach (var node in nodePath)
            {
                var position = _gridManager.GridToWorld(new Vector2Int(node.X, node.Y));
                Path.Add(new Vector3(position.x, 0.1f, position.z));
            }
        }
        
    }
}
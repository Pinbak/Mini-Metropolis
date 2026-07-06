using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Industrial : Building
    {
        
        public new void Init(BuildingManager buildingManager)
        {
            base.Init(buildingManager);

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
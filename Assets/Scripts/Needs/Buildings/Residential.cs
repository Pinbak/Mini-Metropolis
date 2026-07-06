using System;
using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Residential : Building
    {
        // the people that live here
        [SerializeField] private Commuter commuterPrefab;
        [SerializeField] private float workNeed = 100f;

        private Commuter _commuter;

        private void Update()
        {
            workNeed -= Time.deltaTime;
            if (workNeed < 50f)
            {
                if (_commuter.Currently is State.AtHome)
                    _commuter.GoToWork();
            }
        }

        private void ArrivedAtWork()
        {
            workNeed += 50f;
        }

        public new void Init(BuildingManager buildingManager)
        {
            base.Init(buildingManager);
            
            var commuterParkingSpace = ParkingSpaces[0];
            _commuter = Instantiate(commuterPrefab, commuterParkingSpace.transform.position, Quaternion.identity,
                transform);
            _commuter.Init(this, buildingManager, buildingManager.GridManager, buildingManager.IntersectionManager,
                buildingManager.CarAcceleration, commuterParkingSpace);
            _commuter.ArrivedAtWork += ArrivedAtWork;

        }

        #region Draw Gizmos
        
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
        
        #endregion
    }
}
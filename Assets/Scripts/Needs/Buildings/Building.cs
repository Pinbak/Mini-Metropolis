using System;
using System.Collections.Generic;
using System.Linq;
using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public abstract class Building : MonoBehaviour
    {
        [field:SerializeField] public Building UpgradesTo { get; set; }
        [field:SerializeField] public int Width { get; set; }
        [field:SerializeField] public int Height { get; set; }
        [field:SerializeField] public ParkingSpace[] ParkingSpaces { get; set; }
        [SerializeField] private Agent[] supplies;
        [SerializeField] private Agent[] demands;
        [SerializeField] private float startingNeed;
        
        public NodeType[,] Layout { get; private set; }
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }
        public Dictionary<AgentType, float> Needs { get; set; } = new();
        public Dictionary<AgentType, float> NeedUpgradeThresholds { get; set; } = new();
        public Dictionary<AgentType, float> NeedDowngradeThresholds { get; set; } = new();
        
        protected BuildingManager BuildingManager { get; private set; }

        public void Init(BuildingManager buildingManager)
        {
            var position = transform.position;
            BuildingManager = buildingManager;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            
            var gridManager = buildingManager.GridManager;
            BottomLeft = bottomLeft;
            WorldPosition = gridManager.NodeToWorld(bottomLeft);
            
            GenerateLayout();

            for (var i = 0; i < supplies.Length; i++)
            {
                var agentPrefab = supplies[i];
                var agent = Instantiate(agentPrefab, ParkingSpaces[i].transform.position, Quaternion.identity, transform);
                Needs[agent.AgentType] = startingNeed;
                NeedUpgradeThresholds[agent.AgentType] = agent.UpgradeThreshold;
                NeedDowngradeThresholds[agent.AgentType] = agent.DowngradeThreshold;
                agent.Init(this, buildingManager, ParkingSpaces[i]);
            }

            foreach (var demand in demands)
            {
               Needs[demand.AgentType] = startingNeed;
               NeedUpgradeThresholds[demand.AgentType] = demand.UpgradeThreshold;
               NeedDowngradeThresholds[demand.AgentType] = demand.DowngradeThreshold;
            }
        }

        private void Update()
        {
            foreach (var need in Needs.Keys.ToList())
            {
                Needs[need] -= Time.deltaTime;
                if (Needs[need] > NeedUpgradeThresholds[need])
                {
                    // upgrade building
                }
                else if (Needs[need] < NeedDowngradeThresholds[need])
                {
                    // downgrade or destroy building
                }
            }
            
        }

        private void GenerateLayout()
        {
            Layout = new NodeType[Width, Height];
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                Layout[x, y] = NodeType.Building;
            }

            foreach (var parkingSpace in ParkingSpaces)
            {
                var parkingPosition = BuildingManager.GridManager.WorldToNode(parkingSpace.ParentPosition);
                // have to shift real world position by buildings position for relative local position
                var localGridPosition =
                    new Vector2Int(parkingPosition.X - BottomLeft.X, parkingPosition.Y - BottomLeft.Y);
                Layout[localGridPosition.x, localGridPosition.y] = NodeType.Parking;
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
using System.Collections.Generic;
using System.Linq;
using Agents;
using UnityEngine;

namespace Buildings
{
    public class Building : MonoBehaviour
    {
        [field:SerializeField] public BuildingType Type { get; set; }
        [field:SerializeField] public int Cost { get; set; }
        [field:SerializeField] public Building UpgradesTo { get; set; }
        [field:SerializeField] public Building DowngradesTo { get; set; }
        [field:SerializeField] public bool IsGrowable { get; set; }
        public ParkingSpace[] ParkingSpaces { get; private set; }
        [SerializeField] private Agent[] supplies;
        [SerializeField] private Agent[] demands;
        private Agent[] _agents;
        public bool ToRemove { get; private set; }

        public LayoutPosition[] Layout { get; private set; }
        public Node BottomLeft { get; private set; }
        public Vector3 WorldPosition { get; set; }

        [field:SerializeField] public List<Need> Supplies { get; private set; } = new();
        [field:SerializeField] public List<Need> Demands { get; private set; } = new();
        
        protected BuildingManager BuildingManager { get; private set; }
        private bool _isChanging;

        private void Awake()
        {
            ParkingSpaces = GetComponentsInChildren<ParkingSpace>();
            Layout = GetComponentsInChildren<LayoutPosition>();
        }

        public void Init(BuildingManager buildingManager)
        {
            var position = transform.position;
            BuildingManager = buildingManager;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            
            var gridManager = buildingManager.GridManager;
            BottomLeft = bottomLeft;
            WorldPosition = gridManager.NodeToWorld(bottomLeft);
            _agents = new Agent[supplies.Length];
            
            SetupNeeds();

            for (var i = 0; i < supplies.Length; i++)
            {
                var agentPrefab = supplies[i];
                var agent = Instantiate(agentPrefab, ParkingSpaces[i].transform.position, transform.rotation,
                    transform);
                agent.Init(this, buildingManager, ParkingSpaces[i]);
                _agents[i] = agent;
            }
            
        }

        public void RemoveBuilding()
        {
            foreach (var parkingSpace in ParkingSpaces)
                parkingSpace.ParkedAgent?.TeleportToPrimary();
            foreach (var agent in _agents)
                agent.PathMover.TeleportToPrimary();
            ToRemove = true;
        }

        private void Update()
        {
            if (ToRemove) return;
            foreach (var supply in Supplies)
            {
                supply.Update();
            }
            
            foreach (var demand in Demands)
            {
                demand.Update();
            }
        }

        private void SetupNeeds()
        {
            var uniqueDemands = demands.ToHashSet();
            var uniqueSupplies = supplies.ToHashSet();

            foreach (var uniqueSupply in uniqueSupplies)
            {
                var need = new Need();
                need.Init(uniqueSupply);
                need.GettingLow += NeedGettingLow;
                if (IsGrowable)
                {
                    need.AboveThreshold += UpgradeBuilding;
                    need.BelowThreshold += DowngradeBuilding;
                }
                Supplies.Add(need);
            }
            
            foreach (var uniqueDemand in uniqueDemands)
            {
                var need = new Need();
                need.Init(uniqueDemand);
                need.GettingLow += NeedGettingLow;
                if (IsGrowable)
                {
                    need.AboveThreshold += UpgradeBuilding; // todo needs to unsubscribe?
                    need.BelowThreshold += DowngradeBuilding;
                }
                Demands.Add(need);
            }
            
        }

        public void GoTo(Building building, AgentType need)
        {
            foreach (var agent in _agents)
            {
                if (agent.AgentType == need)
                {
                    if (agent.AgentState is AtPrimary)
                    {
                        agent.GoTo(building);
                        return; // only one should go
                    }
                }
            }
        }

        public void IncrementNeed(Agent need, float amount)
        {
            
            foreach (var supply in Supplies)
            {
                if (supply.Type != need.AgentType) continue;
                supply.Increase(amount);
                BuildingManager.Balance += need.Income;
                return;
            }
            
            foreach (var demand in Demands)
            {
                if (demand.Type != need.AgentType) continue;
                demand.Increase(amount / ParkingSpaces.Length);
                return;
            }
            
        }
        
        private void DowngradeBuilding(Need need)
        {
            if (DowngradesTo is null || _isChanging) return;
            _isChanging = true;
            BuildingManager.ChangeBuilding(this, DowngradesTo);
        }

        private void UpgradeBuilding(Need need)
        {
            if (UpgradesTo is null || _isChanging) return;
            _isChanging = true;
            if (Supplies.Any(supply => !supply.IsAboveThreshold()))return;
            if (Demands.Any(demand => !demand.IsAboveThreshold()))return;
            BuildingManager.ChangeBuilding(this, UpgradesTo);
        }

        private void NeedGettingLow(Need need)
        {
            foreach (var agent in _agents)
            {
                
                if (agent.AgentType == need.Type)
                {
                    if (agent.AgentState is AtPrimary && !agent.InQueue)
                    {
                        agent.InQueue = true;
                        BuildingManager.AddToSupplyQueue(this, need);
                    }
                }
            }

            foreach (var agent in demands)
            {
                if (agent.AgentType == need.Type)
                {
                    if (GetFreeParkingSpace(out var space))
                    {
                        space.Queue();
                        BuildingManager.AddToDemandQueue(this, need);
                    }
                }
            }
        }

        public bool GetFreeParkingSpace(out ParkingSpace freeParkingSpace)
        {
            freeParkingSpace = null;
            foreach (var parkingSpace in ParkingSpaces)
            {
                if (parkingSpace.IsBeingTaken || parkingSpace.InQueue ||
                    parkingSpace.ParkedAgent is not null) continue;
                freeParkingSpace = parkingSpace;
                return true;
            }
            
            return false;
        }

        public void GetReservedParkingSpace(out ParkingSpace reservedSpace)
        {
            reservedSpace = null;
            foreach (var parkingSpace in ParkingSpaces)
                if (parkingSpace.InQueue)
                    reservedSpace = parkingSpace;
        }
        
        // private void OnDrawGizmos()
        // {
        //     if (BottomLeft is null) return;
        //     for (var x = 0; x < Width; x++)
        //     for (var y = 0; y < Height; y++)
        //     {
        //         var gridPosition = new Vector2Int(BottomLeft.X + x, BottomLeft.Y + y);
        //         var node = BuildingManager.GridManager.Grid[gridPosition.x, gridPosition.y];
        //         var worldPosition = BuildingManager.GridManager.NodeToWorld(node);
        //         Gizmos.color = Color.red;
        //
        //         if (node.Type is NodeType.Parking)
        //             Gizmos.color = Color.blue;
        //         
        //         Gizmos.DrawSphere(new Vector3(worldPosition.x, worldPosition.y + 1f, worldPosition.z), .1f);
        //     }
        //     
        // }
    }
}
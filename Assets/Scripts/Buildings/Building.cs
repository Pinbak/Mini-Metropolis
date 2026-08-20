using System.Collections.Generic;
using System.Linq;
using Agents;
using UnityEngine;

namespace Buildings
{
    /// <summary>
    ///     A building is something that can be placed directly by the player, or is grown from a <see cref="Zone"/>.
    ///     This class manages the <see cref="Need"/>s, and the <see cref="Agent"/>s that belong to it.
    ///     Used in prefabs to define initial properties, such as the <see cref="Cost"/>, <see cref="ParkingSpaces"/>, and
    ///     what the building <see cref="supplies"/> and <see cref="demands"/>.
    /// </summary>
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

        public Vector3 Top
        {
            get
            {
                // get the highest y position in the layout which is effectively the top of the bounding box
                var max = Layout.Select(layoutPosition => layoutPosition.transform.position.z).Prepend(-Mathf.Infinity).Max();
                return new Vector3(transform.position.x, transform.position.y, max);
            }
        }

        [field:SerializeField] public List<Need> Supplies { get; private set; } = new();
        [field:SerializeField] public List<Need> Demands { get; private set; } = new();

        private BuildingManager BuildingManager { get; set; }
        private bool _isChanging;

        private void Awake()
        {
            // gathers all the child objects that are parking spaces and layout positions
            ParkingSpaces = GetComponentsInChildren<ParkingSpace>();
            Layout = GetComponentsInChildren<LayoutPosition>();
        }

        /// <summary>
        ///     Used instead of a constructor, as it's a MonoBehaviour, so can't really have one. This method sets up the building,
        ///     such as its needs and the agents that it needs to create as well.
        /// </summary>
        /// <param name="buildingManager"></param>
        public void Init(BuildingManager buildingManager)
        {
            var position = transform.position;
            BuildingManager = buildingManager;
            var bottomLeft = buildingManager.GridManager.WorldToNode(position);
            
            var gridManager = buildingManager.GridManager;
            BottomLeft = bottomLeft; // the position is defined by the bottom left of the prefab, this is for consistency
            _agents = new Agent[supplies.Length]; // start to set up the agents
            
            SetupNeeds();
            
            // if the building is supplying a need, the agents get created and managed here
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
            // unsubscribe from need events
            foreach (var need in Supplies)
            {
                need.GettingLow -= NeedGettingLow;
                if (!IsGrowable) continue;
                need.AboveThreshold -= UpgradeBuilding;
                need.BelowThreshold -= DowngradeBuilding;
            }
            
            foreach (var need in Demands)
            {
                need.GettingLow -= NeedGettingLow;
                if (!IsGrowable) continue;
                need.AboveThreshold -= UpgradeBuilding;
                need.BelowThreshold -= DowngradeBuilding;
            }
            
            // for all agents that are parked here or about to be, move them back to their primary location
            foreach (var parkingSpace in ParkingSpaces)
            {
                parkingSpace.ParkedAgent?.TeleportToPrimaryAndRemoveFromJunction();
                parkingSpace.IsBeingTakenAgent?.TeleportToPrimaryAndRemoveFromJunction();
            }
            // all the agents that belong to this object are prepared for deletion, such as removing them from any junction queues, parking spaces etc.
            foreach (var agent in _agents)
                agent.PathMover.PrepareForDeletion();
            ToRemove = true; // flagged to remove
        }

        private void Update()
        {
            if (ToRemove) return;
            // update the needs simulation
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

            // go through the predefined supplies and add the unique ones to the Supplies list of needs
            foreach (var uniqueSupply in uniqueSupplies)
            {
                var need = new Need();
                need.Init(uniqueSupply);
                // subscribe to the actions which are invoked based on the need's thresholds
                need.GettingLow += NeedGettingLow;
                if (IsGrowable)
                {
                    need.AboveThreshold += UpgradeBuilding;
                    need.BelowThreshold += DowngradeBuilding;
                }
                Supplies.Add(need);
            }
            
            // go through the predefined supplies and add the unique ones to the Supplies list of needs
            foreach (var uniqueDemand in uniqueDemands)
            {
                var need = new Need();
                need.Init(uniqueDemand);
                // subscribe to the actions which are invoked based on the need's thresholds
                need.GettingLow += NeedGettingLow;
                if (IsGrowable)
                {
                    need.AboveThreshold += UpgradeBuilding;
                    need.BelowThreshold += DowngradeBuilding;
                }
                Demands.Add(need);
            }
            
        }

        /// <summary>
        ///     Get an agent from this building to move to another building
        /// </summary>
        public void GoTo(Building building, AgentType need)
        {
            // find available agents
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

        /// <summary>
        ///     Increments need on this building
        /// </summary>
        public void IncrementNeed(Agent need, float amount)
        {
            // find the need and increment it
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
            // change to the downgrade is there is one
            BuildingManager.ChangeBuilding(this, DowngradesTo);
        }

        private void UpgradeBuilding(Need need)
        {
            if (UpgradesTo is null || _isChanging) return;
            // only if all the needs are at the threshold does the building upgrade
            if (Supplies.Any(supply => !supply.IsAboveThreshold())) return;
            if (Demands.Any(demand => !demand.IsAboveThreshold())) return;
            _isChanging = true;
            BuildingManager.ChangeBuilding(this, UpgradesTo);
        }

        private void NeedGettingLow(Need need)
        {
            // when a need is low the building registers its low need in the building manager, which will then pair it
            // off with a relevant building, calling GoTo() when found
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
            
            // same as above for the demand needs
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

        /// <summary>
        ///     Find a free parking space on this building, return true if found one and the parking space as out
        /// </summary>
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

        /// <summary>
        ///     Find a parking space that is reserved
        /// </summary>
        public void GetReservedParkingSpace(out ParkingSpace reservedSpace)
        {
            reservedSpace = null;
            foreach (var parkingSpace in ParkingSpaces)
                if (parkingSpace.InQueue)
                    reservedSpace = parkingSpace;
        }
    }
}
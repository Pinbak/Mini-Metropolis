using Needs.Agents;
using UnityEngine;

namespace Needs.Buildings
{
    public class Residential : MonoBehaviour
    {
        // the people that live here
        [SerializeField] private Commuter commuterPrefab;
        [SerializeField] private BuildingType type;
        [SerializeField] private GameObject[] validParkingSpaces;

        private Building _buildingInformation;
        private const int Width = 2;
        private const int Height = 1;

        private Commuter _commuter;

        private void Update()
        {
            // throw new NotImplementedException();
        }

        public void Init(Vector3 position, BuildingManager buildingManager)
        {
            _buildingInformation =
                new Building(Width, Height, buildingManager.GridManager.WorldToNode(position), position);
            foreach (var parkingSpace in validParkingSpaces)
            {
                _commuter = Instantiate(commuterPrefab, parkingSpace.transform.position, Quaternion.identity,
                    transform);
                _commuter.Init(buildingManager, buildingManager.GridManager, buildingManager.IntersectionManager,
                    buildingManager.CarAcceleration);
            }
        }
        
    }
}
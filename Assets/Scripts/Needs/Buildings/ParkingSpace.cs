using UnityEngine;

namespace Needs.Buildings
{
    public class ParkingSpace : MonoBehaviour
    {
        [Tooltip("Which tile this parking space is considered positioned on.")]
        [field:SerializeField] public GameObject ParentPositionGameObject { get; set; }
        public Vector3 ParentPosition => ParentPositionGameObject.transform.position;
        [Tooltip("Where the car will enter and exit the parking space from and to.")]
        
        [field:SerializeField] public GameObject RoadConnectionGameObject { get; set; }

        public Vector3 RoadConnection => RoadConnectionGameObject.transform.position;
        
        [Tooltip("Whether there is anyone parked here.")]
        [field:SerializeField] public bool IsFree { get; set; }
        [field:SerializeField] public bool IsBeingTaken { get; set; }
        [field:SerializeField] public PathMover ParkedAgent { get; set; }
        
    }
}
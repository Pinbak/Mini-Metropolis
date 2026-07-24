using Agents;
using UnityEngine;

namespace Buildings
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
        [field:SerializeField] public bool IsFree { get; private set; }
        [field:SerializeField] public bool IsBeingTaken { get; private set; }
        public PathMover ParkedAgent { get; private set; }
        public PathMover IsBeingTakenAgent { get; private set; }
        [field:SerializeField] public bool InQueue { get; private set; }

        public void Park(PathMover agent)
        {
            IsFree = false;
            IsBeingTaken = false;
            IsBeingTakenAgent = null;
            ParkedAgent = agent;
        }

        public void Leave()
        {
            IsFree = true;
            ParkedAgent = null;
            IsBeingTakenAgent = null;
            IsBeingTaken = false;
        }

        public void Queue() => InQueue = true;
        public void Dequeue() => InQueue = false;

        public void Reserve(PathMover agent)
        {
            IsBeingTakenAgent = agent;
            IsBeingTaken = true;
        }
    }
}
using UnityEngine;

namespace Needs.Agents
{
    public class TravellingToPrimary : IAgentState
    {
        private bool _foundPath;
        private Agent _context;
        
        public void Update(Agent context)
        {
            if (!_foundPath) return;
            context.PathMover.MoveAlongPath(context.MovementSpeed, context.CarAcceleration);
        }

        public void EnterState(Agent context)
        {
            _foundPath = false;
            if (context.PathMover.HasValidPath) Debug.Log("Attempting to travel home while travelling");
            if (!context.PrimaryLocation.GetFreeParkingSpace(out var parkingSpace)) return;
            context.PathMover.GeneratePath(parkingSpace);
            context.PathMover.Arrived += Arrived;
            _context = context;
            if (context.PathMover.HasValidPath)
                _foundPath = true;
        }

        private void Arrived(Node node)
        {
            _context.ChangeState(_context.AtPrimary);
        }

        public void ExitState(Agent context)
        {
            context.PathMover.Arrived -= Arrived;
        }
    }
}
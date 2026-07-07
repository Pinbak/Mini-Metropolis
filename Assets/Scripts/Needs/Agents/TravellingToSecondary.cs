using UnityEngine;

namespace Needs.Agents
{
    public class TravellingToSecondary : IAgentState
    {
        private bool _foundPath;
        private Agent _context;
        private float _timeSpentRetrying;
        
        public void Update(Agent context)
        {
            if (!_foundPath)
            {
                _timeSpentRetrying += Time.deltaTime;
                if (_timeSpentRetrying > context.TimeToWaitUntilRetryingRoute)
                {
                    _timeSpentRetrying = 0f;
                    AttemptMoveToSecondaryLocation(context);
                }

                return;
            }
            context.PathMover.MoveAlongPath(context.MovementSpeed, context.CarAcceleration);
        }

        private void AttemptMoveToSecondaryLocation(Agent context)
        {
            _foundPath = false;
            if (context.PathMover.HasValidPath) Debug.Log("Attempting to travel to work while travelling");
            if (!context.SecondaryLocation.GetFreeParkingSpace(out var parkingSpace)) return;
            context.PathMover.GeneratePath(parkingSpace);
            
            if (context.PathMover.HasValidPath)
            {
                context.PathMover.Arrived += Arrived;
                _foundPath = true;
            }
        }

        public void EnterState(Agent context)
        {
            _context = context;
            AttemptMoveToSecondaryLocation(context);
        }

        private void Arrived(Node node)
        {
            _context.ChangeState(_context.AtSecondary);
        }

        public void ExitState(Agent context)
        {
            context.PathMover.Arrived -= Arrived;
        }
    }
}
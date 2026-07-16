using UnityEngine;

namespace Agents
{
    public class TravellingToPrimary : IAgentState
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
                    AttemptMoveToPrimaryLocation(context);
                }

                return;
            }
            context.PathMover.MoveAlongPath(context.MovementSpeed, context.CarAcceleration);
        }

        private void AttemptMoveToPrimaryLocation(Agent context)
        {
            _foundPath = false;
            if (!context.PrimaryLocation.GetFreeParkingSpace(out var parkingSpace)) return;
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
            context.Returning();
            AttemptMoveToPrimaryLocation(context);

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
using UnityEngine;

namespace Agents
{
    /// <summary>
    ///     The state used when an agent is currently travelling to its primary location (often home).
    /// </summary>
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
                // every x seconds, retry going home, as there should be a valid space, but the road network may have been modified since.
                // This should prevent an agent permanently getting stuck at its secondary location
                if (_timeSpentRetrying > context.TimeToWaitUntilRetryingRoute)
                {
                    _timeSpentRetrying = 0f;
                    AttemptMoveToPrimaryLocation(context);
                }

                return;
            }
            // move along the path that was generated
            context.PathMover.MoveAlongPath(context.MovementSpeed, context.CarAcceleration);
        }

        private void AttemptMoveToPrimaryLocation(Agent context)
        {
            _foundPath = false;
            if (!context.PrimaryLocation.GetFreeParkingSpace(out var parkingSpace)) return;
            context.PathMover.GeneratePath(parkingSpace);
            
            // a space at the primary location is effectively guaranteed, but still have to check regardless
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
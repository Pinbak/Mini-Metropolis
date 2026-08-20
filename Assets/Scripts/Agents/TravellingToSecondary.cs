
namespace Agents
{
    /// <summary>
    ///     The state used when an agent is currently travelling to its secondary location.
    /// </summary>
    public class TravellingToSecondary : IAgentState
    {
        private bool _foundPath;
        private Agent _context;
        private float _timeSpentRetrying;
        
        public void Update(Agent context)
        {
            if (!_foundPath) return;
            // every tick, move along path if found one
            context.PathMover.MoveAlongPath(context.MovementSpeed, context.CarAcceleration);
        }

        private void AttemptMoveToSecondaryLocation(Agent context)
        {
            _foundPath = false;
            // find the destination
            context.SecondaryLocation.GetReservedParkingSpace(out var parkingSpace);
            context.PathMover.GeneratePath(parkingSpace);
            parkingSpace.Dequeue();
            
            if (context.PathMover.HasValidPath)
            {
                context.PathMover.Arrived += Arrived;
                _foundPath = true;
            }
            else
            {
                // failed to find a path
                context.Returning();
                _context.ChangeState(_context.AtPrimary);
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
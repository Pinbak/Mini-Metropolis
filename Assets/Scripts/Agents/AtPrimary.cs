namespace Agents
{
    /// <summary>
    ///     The state of the agent when waiting at its primary location, such as at home.
    /// </summary>
    public class AtPrimary : IAgentState
    {
        public void Update(Agent context)
        {
            // only change state when a valid secondary location has been chosen. This happens when the building the
            // agent belongs to gets the GoTo() method called
            if (context.SecondaryLocation is not null)
                context.ChangeState(context.TravellingToSecondary);
        }

        public void EnterState(Agent context)
        {
        }

        public void ExitState(Agent context)
        {
        }
    }
}
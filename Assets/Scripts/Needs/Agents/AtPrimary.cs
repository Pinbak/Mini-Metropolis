using UnityEngine;

namespace Needs.Agents
{
    public class AtPrimary : IAgentState
    {
        
        public void Update(Agent context)
        {
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
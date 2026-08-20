using UnityEngine;

namespace Agents
{
    /// <summary>
    ///     The state defined as the agent waiting at its secondary location.
    /// </summary>
    public class AtSecondary : IAgentState
    {
        private float _timeSpent;
        
        public void Update(Agent context)
        {
            // wait here until a certain amount of time has elapsed
            _timeSpent += Time.deltaTime;
            if (_timeSpent > context.TimeToSpendAtSecondary)
                context.ChangeState(context.TravellingToPrimary);
        }

        public void EnterState(Agent context)
        {
            _timeSpent = 0f;
        }

        public void ExitState(Agent context)
        {
            context.PrimaryLocation?.IncrementNeed(context, context.NeedIncrease); // just finished work ect.
            context.SecondaryLocation?.IncrementNeed(context, context.NeedIncrease); // just finished work ect.
        }
    }
}
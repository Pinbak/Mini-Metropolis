using UnityEngine;

namespace Agents
{
    public class AtSecondary : IAgentState
    {
        private float _timeSpent;
        
        public void Update(Agent context)
        {
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
            context.PrimaryLocation.IncrementNeed(context, context.NeedIncrease); // just finished work ect.
            context.SecondaryLocation.IncrementNeed(context, context.NeedIncrease); // just finished work ect.
        }
    }
}
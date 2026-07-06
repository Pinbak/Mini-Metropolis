using UnityEngine;

namespace Needs.Agents
{
    public class AtPrimary : IAgentState
    {
        private float _timeSpent;
        
        public void Update(Agent context)
        {
            _timeSpent += Time.deltaTime;
            if (_timeSpent > context.TimeToSpendAtPrimary)
                context.ChangeState(context.TravellingToSecondary);
        }

        public void EnterState(Agent context)
        {
            _timeSpent = 0f;
        }

        public void ExitState(Agent context)
        {
        }
    }
}
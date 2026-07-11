namespace Agents
{
    public interface IAgentState
    {
        void Update(Agent context);
        void EnterState(Agent context);
        void ExitState(Agent context);
    }
}
namespace Agents
{
    /// <summary>
    ///     The interfaces that exposes the methods used for the state pattern. Used for the agent's finite-state machine.
    /// </summary>
    public interface IAgentState
    {
        void Update(Agent context);
        void EnterState(Agent context);
        void ExitState(Agent context);
    }
}
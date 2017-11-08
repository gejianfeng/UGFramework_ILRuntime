namespace UGFramework.Core
{
    public interface IState
    {
        string StateName { get; }
        void OnEnter(StateMachine FSM, params object[] Params);
        void OnUpdate(StateMachine FSM, float DeltaSecond);
        void OnExit(StateMachine FSM, params object[] Params);
    }
}
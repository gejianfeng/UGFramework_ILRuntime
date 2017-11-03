namespace PureMVC.UGFramework.Core
{
    public class State: IState
    {
        public const string NAME = "State";

        protected string m_StateName = string.Empty;

        public State(): this(NAME)
        {

        }

        public State(string StateName)
        {
            m_StateName = string.IsNullOrEmpty(StateName) ? NAME : StateName;
        }

        public string StateName
        {
            get
            {
                return m_StateName;
            }
        }

        public virtual void OnEnter(StateMachine FSM, params object[] Params)
        {
            
        }

        public virtual void OnExit(StateMachine FSM, params object[] Params)
        {
            
        }

        public virtual void OnUpdate(StateMachine FSM, float DeltaSecond)
        {
            
        }
    }
}
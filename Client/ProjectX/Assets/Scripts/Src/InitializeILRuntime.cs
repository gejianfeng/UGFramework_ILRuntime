namespace PureMVC.Project
{
    using PureMVC.UGFramework.Core;

    public class InitializeILRuntime : State
    {
        public new const string NAME = "InitializeILRuntime";

        private bool m_bFinished = false;

        public InitializeILRuntime() : base(NAME)
        {

        }

        public override void OnEnter(StateMachine FSM, params object[] Params)
        {
            base.OnEnter(FSM, Params);
        }

        public override void OnUpdate(StateMachine FSM, float DeltaSecond)
        {
            base.OnUpdate(FSM, DeltaSecond);
        }

        public override void OnExit(StateMachine FSM, params object[] Params)
        {
            base.OnExit(FSM, Params);
        }
    }
}

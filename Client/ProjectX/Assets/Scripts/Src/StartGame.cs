namespace PureMVC.Project
{
    using PureMVC.UGFramework.Core;
    using System;

    public class StartGame : State
    {
        public new const string NAME = "StartGame";

        private bool m_bFinished = false;

        public StartGame() : base(NAME)
        {
            
        }

        public override void OnEnter(StateMachine FSM, params object[] Params)
        {
            base.OnEnter(FSM, Params);

            m_bFinished = true;
        }

        public override void OnUpdate(StateMachine FSM, float DeltaSecond)
        {
            base.OnUpdate(FSM, DeltaSecond);

            if (m_bFinished)
            {
                
            }
        }

        public override void OnExit(StateMachine FSM, params object[] Params)
        {
            base.OnExit(FSM, Params);
        }
    }
}

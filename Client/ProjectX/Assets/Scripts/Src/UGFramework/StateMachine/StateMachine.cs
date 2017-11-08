namespace UGFramework.Core
{
    using System.Collections.Generic;

    public class StateMachine
    {
        protected Dictionary<string, IState> m_StateDict = null;
        protected object m_Actor = null;
        protected IState m_CurrentState = null;

        public object Actor
        {
            get
            {
                return m_Actor;
            }

            set
            {
                m_Actor = value;
            }
        }

        public StateMachine()
        {
            m_StateDict = new Dictionary<string, IState>();
        }

        ~StateMachine()
        {
            if (m_StateDict != null)
            {
                m_StateDict.Clear();
                m_StateDict = null;
            }

            m_Actor = null;
        }

        public void AddState(IState[] StateArray)
        {
            if (m_StateDict == null || StateArray == null)
            {
                return;
            }

            for (int i = 0; i < StateArray.Length; ++i)
            {
                IState _State = StateArray[i];

                if (_State != null && !m_StateDict.ContainsKey(_State.StateName))
                {
                    m_StateDict.Add(_State.StateName, _State);
                }
            }
        }

        public void AddState(List<IState> StateArray)
        {
            if (m_StateDict == null || StateArray == null)
            {
                return;
            }

            AddState(StateArray.ToArray());
        }

        public void AddState(IState NewState)
        {
            if (m_StateDict == null || NewState == null)
            {
                return;
            }

            if (!m_StateDict.ContainsKey(NewState.StateName))
            {
                m_StateDict.Add(NewState.StateName, NewState);
            }
        }

        public void ReplaceState(IState NewState)
        {
            if (NewState == null || m_StateDict == null || m_CurrentState == NewState)
            {
                return;
            }

            if (!m_StateDict.ContainsKey(NewState.StateName))
            {
                m_StateDict.Add(NewState.StateName, NewState);
            }
            else
            {
                m_StateDict[NewState.StateName] = NewState;
            }
        }

        public void Update(float DeltaSecond)
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(this, DeltaSecond);
            }
        }

        public void SwitchState(string StateName, params object[] Param)
        {
            if (string.IsNullOrEmpty(StateName) || m_StateDict == null || !m_StateDict.ContainsKey(StateName) || m_StateDict[StateName] == null)
            {
                return;
            }

            IState _NewState = m_StateDict[StateName];

            if (m_CurrentState == _NewState)
            {
                return;
            }

            if (m_CurrentState != null)
            {
                m_CurrentState.OnExit(this, Param);
            }
            
            m_CurrentState = _NewState;
            m_CurrentState.OnEnter(this, Param);
        }
    }
}
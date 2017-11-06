namespace PureMVC.Project
{
    using UnityEngine;
    using UnityEngine.UI;
    using PureMVC.UGFramework.Core;
    using System;

    public class MainUI: MonoBehaviour
    {
        private StateMachine m_StateMachine = null;
        private bool m_bDebug = false;

        private Text m_HintText = null;

        public bool IsDebug
        {
            get
            {
                return m_bDebug;
            }
        }

        void Awake()
        {
            m_HintText = GameObject.Find("RootCanvas/txtHint").GetComponent<Text>();

            m_bDebug = CheckInDebugEnv();

            m_StateMachine = new StateMachine();
            m_StateMachine.Actor = this;

            if (m_bDebug)
            {
                RunInDeviceMode();
            }
            else
            {
                RunInEditorMode();
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        void Update()
        {
            if (m_StateMachine != null)
            {
                m_StateMachine.Update(Time.deltaTime);
            }
        }

        private bool CheckInDebugEnv()
        {
            return Type.GetType("PureMVC.Project.Define.GameConst") == null;
        }

        private void RunInEditorMode()
        {
            if (m_StateMachine == null)
            {
                return;
            }

            State[] _StateList = new State[]
            {
                new CopyLocalDb(string.Empty)
            };

            m_StateMachine.AddState(_StateList);
        }

        private void RunInDeviceMode()
        {
            if (m_StateMachine == null)
            {
                return;
            }

            State[] _StateList = new State[]
            {
                new CopyLocalDb(string.Empty),
                new CopyLocalGameData(),
                new InitializeILRuntime()
            };

            m_StateMachine.AddState(_StateList);
        }

        public void SetHint(string Content)
        {
            if (m_HintText != null)
            {
                m_HintText.text = Content;
            }
        }
    }
}

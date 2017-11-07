namespace PureMVC.Project
{
    using UnityEngine;
    using UnityEngine.UI;
    using PureMVC.UGFramework.Core;
    using System;
    using System.IO;

    public class MainUI: MonoBehaviour
    {
        private StateMachine m_StateMachine = null;
        private bool m_bDebug = false;

        private Text m_HintText = null;

        public static string DbSrcPath {  get { return Path.Combine(Application.streamingAssetsPath, "db"); } }
        public static string DbDestPath {  get { return Path.Combine(Application.persistentDataPath, "db"); } }
        public static string GameDataSrcPath { get { return Path.Combine(Application.streamingAssetsPath, "data"); } }
        public static string GameDataDestPath { get { return Path.Combine(Application.persistentDataPath, "data"); } }

        public static readonly string[] GameDataFileList = new string[] { "game1.data", "game2.data" };

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
            return Type.GetType("PureMVC.Project.PatchLoader") == null;
        }

        private void RunInEditorMode()
        {
            if (m_StateMachine == null)
            {
                return;
            }

            State[] _StateList = new State[]
            {
                new CopyLocalDb(StartGame.NAME),
                new StartGame()
            };

            m_StateMachine.AddState(_StateList);

            m_StateMachine.SwitchState(CopyLocalDb.NAME);
        }

        private void RunInDeviceMode()
        {
            if (m_StateMachine == null)
            {
                return;
            }

            State[] _StateList = new State[]
            {
                new CopyLocalDb(CopyLocalGameData.NAME),
                new CopyLocalGameData(),
                new InitializeILRuntime(),
                new StartGame()
            };

            m_StateMachine.AddState(_StateList);

            m_StateMachine.SwitchState(CopyLocalDb.NAME);
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

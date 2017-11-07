namespace PureMVC.Project
{
    using ILRuntime.Runtime.Enviorment;
    using PureMVC.UGFramework.Core;
    using System.Collections;
    using System.IO;
    using UnityEngine;

    public class InitializeILRuntime : State
    {
        public new const string NAME = "InitializeILRuntime";

        private string m_GameDataPath = string.Empty;
        private MemoryStream m_Dll = null;
        private MemoryStream m_Pdb = null;
        private bool m_bFinished = false;
        private Coroutine m_LoadCoroutine = null;

        public InitializeILRuntime() : base(NAME)
        {

        }

        public override void OnEnter(StateMachine FSM, params object[] Params)
        {
            base.OnEnter(FSM, Params);

            MainUI _MainUI = null;

            if (FSM != null && FSM.Actor != null)
            {
                _MainUI = FSM.Actor as MainUI;
            }

            if (_MainUI == null)
            {
                return;
            }

            _MainUI.SetHint("Initialize Game Resource");

            m_GameDataPath = MainUI.GameDataDestPath;

            m_LoadCoroutine = _MainUI.StartCoroutine(LoadGameData());
        }

        public override void OnUpdate(StateMachine FSM, float DeltaSecond)
        {
            base.OnUpdate(FSM, DeltaSecond);

            if (m_bFinished && m_Dll != null && m_Pdb != null)
            {
                if (PatchManager.Initialize(m_Dll, m_Pdb))
                {
                    AppDomain _AppDomain = PatchManager.GetAppDomain();

                    if (_AppDomain != null)
                    {
                        _AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
                    }

                    if (FSM != null)
                    {
                        FSM.SwitchState(StartGame.NAME);
                    }
                }
            }
        }

        public override void OnExit(StateMachine FSM, params object[] Params)
        {
            base.OnExit(FSM, Params);

            if (m_LoadCoroutine != null && FSM != null && FSM.Actor != null)
            {
                MainUI _MainUI = FSM.Actor as MainUI;

                if (_MainUI != null)
                {
                    _MainUI.StopCoroutine(m_LoadCoroutine);
                }
            }
        }

        protected IEnumerator LoadGameData()
        {
            yield return null;

            for (int i = 0; i < MainUI.GameDataFileList.Length; ++i)
            {
                string _FileName = MainUI.GameDataFileList[i];
                string _FilePath = Path.Combine(m_GameDataPath, _FileName);

				FileInfo _FileInfo = new FileInfo(_FilePath);

				if (_FileInfo == null || !_FileInfo.Exists)
				{
					yield break;
				}

				byte[] _bytes = File.ReadAllBytes(_FilePath);

				if (i == 0)
				{
					m_Dll = new MemoryStream(_bytes);
				}
				else
				{
					m_Pdb = new MemoryStream(_bytes);
				}

                yield return null;
            }

            m_bFinished = true;
        }
    }
}

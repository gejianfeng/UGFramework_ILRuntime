namespace PureMVC.Project
{
    using PureMVC.UGFramework.Core;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class CopyLocalDb: State
    {
        public new const string NAME = "CopyLocalDb";

        private string m_SrcPath = Application.streamingAssetsPath + "/db";
        private string m_DestPath = Application.persistentDataPath + "/db";

        private string m_NextStateName = string.Empty;

        private List<string> m_ToCopyFileList = null;

        private bool m_bFinished = false;

        private Coroutine m_CopyCoroutine = null;

        public CopyLocalDb(string NextStateName): base(NAME)
        {
            m_NextStateName = NextStateName;
            m_ToCopyFileList = new List<string>();
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

#if UNITY_EDITOR
            if (SystemUtility.IsFileExist(m_DestPath + "/ingame.db"))
            {
                SystemUtility.DeleteFile(m_DestPath + "/ingame.db");
            }
#endif

            if (!SystemUtility.IsFileExist(m_DestPath + "/ingame.db"))
            {
                m_ToCopyFileList.Add("ingame.db");
            }

            if (m_ToCopyFileList.Count == 0)
            {
                m_bFinished = true;
            }
            else
            {
                m_CopyCoroutine = _MainUI.StartCoroutine(CopyFile());
            }
        }

        public override void OnUpdate(StateMachine FSM, float DeltaSecond)
        {
            base.OnUpdate(FSM, DeltaSecond);

            if (m_bFinished)
            {
                FSM.SwitchState(m_NextStateName);
            }
        }

        public override void OnExit(StateMachine FSM, params object[] Params)
        {
            base.OnExit(FSM, Params);

            if (m_CopyCoroutine != null && FSM != null && FSM.Actor != null)
            {
                MainUI _MainUI = FSM.Actor as MainUI;

                if (_MainUI != null)
                {
                    _MainUI.StopCoroutine(m_CopyCoroutine);
                }
            }
        }
        
        private IEnumerator CopyFile()
        {
            yield return null;

            for (int i = 0; i < m_ToCopyFileList.Count; ++i)
            {
                string _SrcPath = m_SrcPath + "/" + m_ToCopyFileList[i];
                string _DestPath = m_DestPath + "/" + m_ToCopyFileList[i];

                WWW _Reader = new WWW(_SrcPath);

                while (!_Reader.isDone)
                {
                    yield return null;
                }

                if (!string.IsNullOrEmpty(_Reader.error))
                {
                    Debug.LogError(_Reader.error);
                    yield break;
                }

                try
                {
                    File.WriteAllBytes(_DestPath, _Reader.bytes);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    yield break;
                }
            }

            m_bFinished = true;
        }
        
    }
}

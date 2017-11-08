namespace ProjectX.Main
{
    using UGFramework.Core;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class CopyLocalDb: State
    {
        public new const string NAME = "CopyLocalDb";

        private string m_SrcPath = string.Empty;
        private string m_DestPath = string.Empty;

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

            m_SrcPath = MainUI.DbSrcPath;
            m_DestPath = MainUI.DbDestPath;

            DirectoryInfo _DirInfo = new DirectoryInfo(m_DestPath);

            if (_DirInfo == null || !_DirInfo.Exists)
            {
                Directory.CreateDirectory(m_DestPath);
            }

            string _DestDbPath = Path.Combine(m_DestPath, "ingame.db");

#if UNITY_EDITOR
            if (SystemUtility.IsFileExist(_DestDbPath))
            {
                SystemUtility.DeleteFile(_DestDbPath);
            }
#endif

            if (!SystemUtility.IsFileExist(_DestDbPath))
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
                string _SrcPath = Path.Combine(m_SrcPath, m_ToCopyFileList[i]);
                string _DestPath = Path.Combine(m_DestPath, m_ToCopyFileList[i]);

                if (Application.platform == RuntimePlatform.Android)
                {
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
                else
                {
                    try
                    {
                        File.Copy(_SrcPath, _DestPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                        yield break;
                    }
                }

                yield return null;
            }

            m_bFinished = true;
        }
        
    }
}

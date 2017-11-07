namespace PureMVC.Project
{
    using PureMVC.UGFramework.Core;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class CopyLocalGameData : State
    {
        public new const string NAME = "CopyLocalGameData";

        private string m_SrcPath = string.Empty;
        private string m_DestPath = string.Empty;

        private string[] m_DataFileList = null;

        private List<string> m_ToCopyFileList = null;

        private bool m_bFinished = false;

        private Coroutine m_CopyCoroutine = null;

        public CopyLocalGameData() : base(NAME)
        {
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

            m_SrcPath = MainUI.GameDataSrcPath;
            m_DestPath = MainUI.GameDataDestPath;
            m_DataFileList = MainUI.GameDataFileList;

            DirectoryInfo _DirInfo = new DirectoryInfo(m_DestPath);
            if (_DirInfo == null || !_DirInfo.Exists)
            {
                Directory.CreateDirectory(m_DestPath);
            }

#if UNITY_EDITOR
            for (int i = 0; i < m_DataFileList.Length; ++i)
            {
                string _FileName = m_DataFileList[i];
                string _FilePath = Path.Combine(m_DestPath, _FileName);

                if (string.IsNullOrEmpty(_FileName))
                {
                    continue;
                }

                if (SystemUtility.IsFileExist(_FilePath))
                {
                    SystemUtility.DeleteFile(_FilePath);
                }
            }
#endif
            for (int i = 0; i < m_DataFileList.Length; ++i)
            {
                string _FileName = m_DataFileList[i];
                string _FilePath = Path.Combine(m_DestPath, _FileName);

                if (string.IsNullOrEmpty(_FileName))
                {
                    continue;
                }

                if (!SystemUtility.IsFileExist(_FilePath))
                {
                    m_ToCopyFileList.Add(_FileName);
                }
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
                FSM.SwitchState(InitializeILRuntime.NAME);
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

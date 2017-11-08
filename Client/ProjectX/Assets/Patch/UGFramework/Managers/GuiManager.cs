namespace UGFramework.Core
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectX.Managers;

    public class PanelInfo
    {
        protected string m_Name = string.Empty;
        protected object m_Param = null;
        protected bool m_bAllowInterupted = true;
        
        public object Param
        {
            get
            {
                return m_Param;
            }
        } 

        public bool IsAllowInterupted
        {
            get
            {
                return m_bAllowInterupted;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        protected PanelInfo()
        {

        }

        public PanelInfo(string PanelName, object Param, bool bAllowInterupted)
        {
            m_Name = PanelName;
            m_Param = Param;
            m_bAllowInterupted = bAllowInterupted;
        }
    }

    public class GuiManager: MonoBehaviourMediator
    {
        public new const string NAME = "GuiManager";

        protected Dictionary<string, Dictionary<string, PanelInfo>> m_PanelInfos = null;

        public GuiManager(object ViewComponent): base(NAME, ViewComponent, true)
        {
            m_PanelInfos = new Dictionary<string, Dictionary<string, PanelInfo>>();
        }

        public override void OnRemove()
        {
            base.OnRemove();

            foreach (var _Iterator in m_PanelInfos)
            {
                if (_Iterator.Value == null)
                {
                    continue;
                }

                foreach(var _SubIterator in _Iterator.Value)
                {
                    string _MediatorName = _SubIterator.Key;

                    MonoBehaviourMediator _Mediator = GameFacade.GetMediator<MonoBehaviourMediator>(_MediatorName);

                    if (_Mediator != null && _Mediator.Injector != null)
                    {
                        UnityEngine.Object.Destroy(_Mediator.Injector.gameObject);
                    }
                    else
                    {
                        GameFacade.GetInstance().RemoveMediator(_MediatorName);
                    }

                    m_PanelInfos[_Iterator.Key][_SubIterator.Key] = null;
                }

                m_PanelInfos[_Iterator.Key].Clear();
            }

            m_PanelInfos.Clear();
            m_PanelInfos = null;
        }

        public static void PopupPanel(System.Type MediatorType, string PrefabPath, object Param, bool bAllowDuplicated, bool bAllowInterupted, string CustomName = null)
        {
            GuiManager _GuiMgr = GameFacade.GetMediator<GuiManager>(GuiManager.NAME);

            if (_GuiMgr != null)
            {
                _GuiMgr.PopPanelInternal(MediatorType, PrefabPath, Param, bAllowDuplicated, bAllowInterupted, CustomName);
            }
        }

        public static void ClosePanel(string MediatorName)
        {
            GuiManager _GuiMgr = GameFacade.GetMediator<GuiManager>(GuiManager.NAME);

            if (_GuiMgr != null)
            {
                _GuiMgr.ClosePanelInternal(MediatorName);
            }
        }

        public static void CloseAllowInteruptedClosePanel()
        {
            GuiManager _GuiMgr = GameFacade.GetMediator<GuiManager>(GuiManager.NAME);

            if (_GuiMgr != null)
            {
                _GuiMgr.CloseAllowInteruptedClosePanelInternal();
            }
        }

        protected void PopPanelInternal(System.Type MediatorType, string PrefabPath, object Param, bool bAllowDuplicated, bool bAllowInterupted, string CustomName)
        {
            if (MediatorType == null || string.IsNullOrEmpty(PrefabPath))
            {
                return;
            }

            string _MediatorName = string.IsNullOrEmpty(CustomName) ? MediatorType.Name : CustomName;

            if (!bAllowDuplicated)
            {
                if (GameFacade.GetInstance().HasMediator(_MediatorName))
                {
                    return;
                }
            }
            else
            {
                _MediatorName = _MediatorName + GameManager.GetInstance().IncreaseObjectIndex();
            }

            GameObject _PrefabObject = Resources.Load<GameObject>(PrefabPath);

            if (_PrefabObject == null)
            {
                return;
            }

            GameObject _PanelInstance = UnityEngine.Object.Instantiate<GameObject>(_PrefabObject);

            if (_PanelInstance == null)
            {
                return;
            }

            _PanelInstance.name = _MediatorName;

            GuiMediator _Mediator = System.Activator.CreateInstance(MediatorType) as GuiMediator;

            if (_Mediator == null)
            {
                UnityEngine.Object.Destroy(_PanelInstance);
                return;
            }

            PanelInfo _NewInfo = new PanelInfo(_MediatorName, Param, bAllowInterupted);

            _Mediator.InitializeMediator(_MediatorName, _PanelInstance, _NewInfo);

            if (!m_PanelInfos.ContainsKey(MediatorType.Name) || m_PanelInfos[MediatorType.Name] == null)
            {
                m_PanelInfos.Add(MediatorType.Name, new Dictionary<string, PanelInfo>());
            }

            m_PanelInfos[MediatorType.Name].Add(_MediatorName, _NewInfo);
        }

        protected void ClosePanelInternal(string MediatorName)
        {
            if (string.IsNullOrEmpty(MediatorName))
            {
                return;
            }

            MonoBehaviourMediator _Mediator = GameFacade.GetMediator<MonoBehaviourMediator>(MediatorName);

            if (_Mediator != null && _Mediator.Injector != null)
            {
                UnityEngine.Object.Destroy(_Mediator.Injector.gameObject);
            }
            else
            {
                GameFacade.GetInstance().RemoveMediator(MediatorName);
            }

            foreach (var _Iterator in m_PanelInfos)
            {
                if (_Iterator.Value != null && _Iterator.Value.ContainsKey(MediatorName))
                {
                    m_PanelInfos[_Iterator.Key].Remove(MediatorName);
                }
            }
        }

        public void CloseAllowInteruptedClosePanelInternal()
        {
            foreach (var _Iterator in m_PanelInfos)
            {
                if (_Iterator.Value == null)
                {
                    continue;
                }

                List<string> _ToRemoveList = new List<string>();

                foreach (var _SubIterator in _Iterator.Value)
                {
                    if (_SubIterator.Value != null && _SubIterator.Value.IsAllowInterupted)
                    {
                        string _MediatorName = _SubIterator.Key;
                        _ToRemoveList.Add(_MediatorName);

                        MonoBehaviourMediator _Mediator = GameFacade.GetMediator<MonoBehaviourMediator>(_MediatorName);
                        if (_Mediator != null && _Mediator.Injector != null)
                        {
                            UnityEngine.Object.Destroy(_Mediator.Injector.gameObject);
                        }
                        else
                        {
                            GameFacade.GetInstance().RemoveMediator(_MediatorName);
                        }
                    }
                }

                for (int i = 0; i < _ToRemoveList.Count; ++i)
                {
                    m_PanelInfos[_Iterator.Key].Remove(_ToRemoveList[i]);
                }
            }
        }

        public PanelInfo GetPanelInfoByName(string PanelName)
        {
            if (string.IsNullOrEmpty(PanelName))
            {
                return null;
            }

            foreach (var _Iterator in m_PanelInfos)
            {
                if (_Iterator.Value != null && _Iterator.Value.ContainsKey(PanelName))
                {
                    return _Iterator.Value[PanelName];
                }
            }

            return null;
        }

        public bool IsPanelOpened(string PanelName)
        {
            return GameFacade.GetInstance().HasMediator(PanelName);
        }
    }
}

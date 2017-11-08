namespace ProjectX.Managers
{
    using ILRuntime.Runtime.Enviorment;
    using System.IO;
    using System.Reflection;
    using UnityEngine;

    public class GameManager: MonoBehaviour
    {
        private static GameManager m_Instance = null;
        private bool m_bSingleton = false;

        protected long m_ObjectIndex = 0;

        private AppDomain m_AppDomain = null;

        public long ObjectIndex
        {
            get
            {
                return m_ObjectIndex;
            }
        }

        public static GameManager GetInstance()
        {
            if (m_Instance == null)
            {
                GameObject _RootObject = new GameObject();
                _RootObject.name = "GameManager";
                _RootObject.transform.position = Vector3.zero;
                _RootObject.transform.rotation = Quaternion.identity;
                _RootObject.transform.localScale = Vector3.one;

                DontDestroyOnLoad(_RootObject);

                m_Instance = _RootObject.AddComponent<GameManager>();
            }
            return m_Instance;
        }

        void Awake()
        {
            if (m_Instance != null)
            {
                Destroy(this);
                return;
            }

            m_bSingleton = true;
        }

        void OnDestroy()
        {
            if (m_bSingleton)
            {
                m_Instance = null;
            }
        }

        public AppDomain GetAppDomain()
        {
            return m_AppDomain;
        }

        public bool InitializeAppDomain(MemoryStream DllData, MemoryStream PdbData)
        {
            if (DllData == null || PdbData == null)
            {
                return false;
            }

            if (m_AppDomain != null)
            {
                m_AppDomain = null;
            }

            m_AppDomain = new AppDomain();
            m_AppDomain.LoadAssembly(DllData, PdbData, new Mono.Cecil.Pdb.PdbReaderProvider());

            return true;
        }

        public void SendNotification(string NotificationName, object Param = null)
        {
            System.Type _PatchLoader = System.Type.GetType(("ProjectX.PatchLoader"));

            if (_PatchLoader == null)
            {
                AppDomain _AppDomain = GameManager.GetInstance().GetAppDomain();

                if (_AppDomain != null)
                {
                    _AppDomain.Invoke("ProjectX.PatchLoader", "SendNotification", null, NotificationName, Param);
                }
            }
            else
            {
                MethodInfo _Method = _PatchLoader.GetMethod("SendNotification", BindingFlags.Public | BindingFlags.Static);

                if (_Method != null)
                {
                    object[] _Params = new object[] {NotificationName, Param};
                    _Method.Invoke(null, _Params);
                }
            }
        }

        public long IncreaseObjectIndex()
        {
            long _Ret = m_ObjectIndex;
            m_ObjectIndex++;
            return _Ret;
        }

        public void ResetObjectIndex()
        {
            m_ObjectIndex = 0;
        }
    }
}
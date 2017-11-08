namespace ProjectX.Managers
{
    using ILRuntime.Runtime.Enviorment;
    using System.IO;
    using UnityEngine;

    public class GameManager: MonoBehaviour
    {
        private static GameManager m_Instance = null;
        private bool m_bSingleton = false;

        private AppDomain m_AppDomain = null;

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
    }
}
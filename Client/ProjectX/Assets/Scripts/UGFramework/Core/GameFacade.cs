namespace PureMVC.UGFramework.Core
{
    using PureMVC.Patterns;
    using ILRuntime.Runtime.Enviorment;

    public class GameFacade: Facade
    {
        private static GameFacade m_SingletonInstance = null;

        private AppDomain m_AppDomain = null;

        public AppDomain AppDomain
        {
            get
            {
                return m_AppDomain;
            }

            set
            {
                m_AppDomain = value;
            }
        }

        public static GameFacade GetInstance()
        {
            if (m_SingletonInstance == null)
            {
                lock (m_staticSyncRoot)
                {
                    if (m_SingletonInstance == null)
                    {
                        m_SingletonInstance = new GameFacade();
                    }
                }
            }

            return m_SingletonInstance;
        }

        public static T GetMediator<T>(string MediatorName) where T: BaseMediator
        {
            if (m_SingletonInstance == null)
            {
                return null;
            }

            var _Mediator = m_SingletonInstance.RetrieveMediator(MediatorName);
            return _Mediator as T;
        }

        public static T GetProxy<T>(string ProxyName) where T: Proxy
        {
            if (m_SingletonInstance == null)
            {
                return null;
            }

            var _Proxy = m_SingletonInstance.RetrieveProxy(ProxyName);
            return _Proxy as T;
        }
    } 
}
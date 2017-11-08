namespace UGFramework.Core
{
    using PureMVC.Patterns;
    using ILRuntime.Runtime.Enviorment;
    using System.IO;

    public class GameFacade: Facade
    {
        private static GameFacade m_SingletonInstance = null;

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

        public static void DestroyFacade()
        {
            m_SingletonInstance = null;
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
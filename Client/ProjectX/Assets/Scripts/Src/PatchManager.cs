namespace PureMVC.Project
{
    using ILRuntime.Runtime.Enviorment;
    using System.IO;

    public class PatchManager
    {
        private static AppDomain m_AppDomain = null;
        private object m_StaticSyncLock = new object();

        public static AppDomain GetAppDomain()
        {
            return m_AppDomain;
        }

        public static bool Initialize(Stream DllStream, Stream PdbStream)
        {
            if (DllStream == null || PdbStream == null)
            {
                return false;
            }

            if (m_AppDomain != null)
            {
                m_AppDomain = null;
            }

            m_AppDomain = new AppDomain();
            m_AppDomain.LoadAssembly(DllStream, PdbStream, new Mono.Cecil.Pdb.PdbReaderProvider());

            return true;
        }
    }
}

namespace PureMVC.UGFramework.Core
{
    using System.Collections.Generic;

    public class Dao
    {
        protected string m_DbPath = string.Empty;

        public Dao(string DbPath)
        {
            m_DbPath = DbPath;
        }
    }
}

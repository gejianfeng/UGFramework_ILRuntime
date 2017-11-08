namespace UGFramework.Core
{
    using System.Collections.Generic;

    public class GameScene
    {
        public const string NAME = "GameScene";

        protected string m_SceneName = string.Empty;
        protected object m_Params;
        protected bool m_bBuildIn = false;

        public GameScene(): this(NAME, null, false)
        {

        }

        public GameScene(string SceneName): this(SceneName, null, false)
        {

        }

        public GameScene(string SceneName, object Param, bool IsBuiltIn)
        {
            m_SceneName = SceneName;
            m_Params = Param;
            m_bBuildIn = IsBuiltIn;
        }

        public string SceneName
        {
            get
            {
                return m_SceneName;
            }
        }

        public object Param
        {
            get
            {
                return m_Params;
            }
        }

        public bool IsBuiltIn
        {
            get
            {
                return m_bBuildIn;
            }
        }

        public virtual void OnSceneLoaded(UnityEngine.SceneManagement.Scene InScene)
        {

        }

        public virtual void OnSceneUnloaded(UnityEngine.SceneManagement.Scene InScene)
        {

        }
    }
}
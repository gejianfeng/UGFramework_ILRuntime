namespace UGFramework.Core
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class GameSceneManager: MonoBehaviourMediator
    {
        public new const string NAME = "GameSceneManager";

        public const string EmptySceneName = "Empty";

        protected bool m_bReplacing = false;
        protected AsyncOperation m_AsyncOpt = null;
        protected Coroutine m_SwitchCoroutine = null;

        protected GameScene m_ActiveScene = null;
        protected GameScene m_TargetScene = null;

        public GameSceneManager(object ViewComponent): base(NAME, ViewComponent, true)
        {

        }

        public override void OnRegister()
        {
            base.OnRegister();

            RegisterSceneManagerDelegate();
        }

        public override void OnRemove()
        {
            base.OnRemove();

            RemoveSceneManagerDelegate();
        }

        protected void RegisterSceneManagerDelegate()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected void RemoveSceneManagerDelegate()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        protected void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene LastScene, UnityEngine.SceneManagement.Scene NewScene)
        {
            if (m_TargetScene != null)
            {
                m_ActiveScene = m_TargetScene;
            }
        }

        protected void OnSceneLoaded(UnityEngine.SceneManagement.Scene NewScene, UnityEngine.SceneManagement.LoadSceneMode LoadMode)
        {
            if (m_ActiveScene != null)
            {
                m_ActiveScene.OnSceneLoaded(NewScene);
            }
            
            if (m_SwitchCoroutine != null)
            {
                StopCoroutine(m_SwitchCoroutine);
                m_SwitchCoroutine = null;
            }

            m_bReplacing = false;
        }

        protected void OnSceneUnloaded(UnityEngine.SceneManagement.Scene LastScene)
        {
            if (m_ActiveScene != null)
            {
                m_ActiveScene.OnSceneUnloaded(LastScene);
            }
        }

        public void SwitchScene(GameScene TargetScene)
        {
            if (TargetScene == null || m_bReplacing || m_SwitchCoroutine != null || m_Injector == null)
            {
                return;
            }

            m_bReplacing = true;

            m_TargetScene = TargetScene;

            m_SwitchCoroutine = StartCoroutine(SwitchSceneCoroutine(TargetScene.IsBuiltIn ? TargetScene.SceneName : EmptySceneName));
        }

        protected IEnumerator SwitchSceneCoroutine(string SceneName)
        {
            m_AsyncOpt = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);
            yield return m_AsyncOpt;
        }
    }
}
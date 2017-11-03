namespace PureMVC.UGFramework.Core
{
    using UnityEngine;

    public class MonoBehaviourMediator: BaseMediator
    {
        public new const string NAME = "MonoBehaviourMediator";

        protected MonoInjector m_Injector = null;

        public override object ViewComponent
        {
            get
            {
                return base.ViewComponent;
            }

            set
            {
                if (base.ViewComponent == value)
                {
                    return;
                }

                Eject();
                base.ViewComponent = value;
                Inject();
            }
        }

        public MonoInjector Injector
        {
            get
            {
                return m_Injector;
            }
        }

        public MonoBehaviourMediator(string MediatorName, object ViewComponent, bool bAutoRegister): base(MediatorName, ViewComponent)
        {
            Inject();

            if (bAutoRegister)
            {
                RegisterMediator();
            }

            Awake();
        }

        public override void OnRemove()
        {
            base.OnRemove();

            if (m_Injector != null)
            {
                Eject();
            }
        }

        protected virtual void Inject()
        {
            if (m_Injector != null)
            {
                Eject();
            }

            GameObject _ViewObject = ViewComponent as GameObject;

            if (_ViewObject == null)
            {
                return;
            }

            m_Injector = _ViewObject.AddComponent<MonoInjector>();

            if (m_Injector != null)
            {
                m_Injector.InjectorName = MediatorName;
                BindInjectorDelegate();
            }
        }

        protected virtual void Eject()
        {
            if (m_Injector == null)
            {
                return;
            }

            RemoveInjectorDelegate();

            UnityEngine.Object.Destroy(m_Injector);
            m_Injector = null;
        }

        protected virtual void BindInjectorDelegate()
        {
            if (m_Injector != null)
            {
                m_Injector.OnDestroyDelegate += DestroyHandler;
            }
        }

        protected virtual void RemoveInjectorDelegate()
        {
            if (m_Injector != null)
            {
                m_Injector.OnDestroyDelegate -= DestroyHandler;
            }
        }

        protected void DestroyHandler()
        {
            PreOnDestroy();
            OnDestroy();
            PostOnDestroy();
        }

        protected virtual void PreOnDestroy()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void PostOnDestroy()
        {
            RemoveMediator();
        }

        protected virtual void Awake()
        {

        }
    }
}

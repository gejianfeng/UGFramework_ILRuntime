namespace UGFramework.Core
{
    using UnityEngine;

    public class InputManager: MonoBehaviourMediator
    {
        public new const string NAME = "InputManager";

        public const string OnGprsStateChanged = "OnGprsStateChanged";

        public System.Action UpdateDelegate;

        protected LocationServiceStatus m_LastGprsStatus = LocationServiceStatus.Stopped;

        public InputManager(object ViewComponent): base(NAME, ViewComponent, true)
        {

        }

        public override void OnRegister()
        {
            base.OnRegister();

            m_LastGprsStatus = Input.location.status;
        }

        protected override void BindInjectorDelegate()
        {
            base.BindInjectorDelegate();

            if (m_Injector != null)
            {
                m_Injector.UpdateDelegate += Update;
            }
        }

        protected override void RemoveInjectorDelegate()
        {
            base.RemoveInjectorDelegate();

            if (m_Injector != null)
            {
                m_Injector.UpdateDelegate -= Update;
            }
        }

        public void StartGprs()
        {
            if (!Input.location.isEnabledByUser)
            {
                return;
            }

            if (Input.location.status == LocationServiceStatus.Initializing || Input.location.status == LocationServiceStatus.Running)
            {
                Input.location.Start();
            }
        }

        public void StopGprs()
        {
            if (!Input.location.isEnabledByUser)
            {
                return;
            }

            Input.location.Stop();
        }

        public bool GetLocationInfo(out LocationInfo Info)
        {
            if (Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running)
            {
                Info = Input.location.lastData;
                return true;
            }
            else
            {
                Info = default(LocationInfo);
                return false;
            }
        }

        protected void Update()
        {
            UpdateGprs();

            if (UpdateDelegate != null)
            {
                UpdateDelegate();
            }
        }

        protected void UpdateGprs()
        {
            if (m_LastGprsStatus != Input.location.status)
            {
                GameFacade.GetInstance().SendNotification(OnGprsStateChanged, m_LastGprsStatus);
            }

            m_LastGprsStatus = Input.location.status;
        }
    }
}

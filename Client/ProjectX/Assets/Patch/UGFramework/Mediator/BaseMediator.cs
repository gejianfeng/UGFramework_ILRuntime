namespace UGFramework.Core
{
    using System;
    using System.Collections.Generic;
    using PureMVC.Patterns;
    using PureMVC.Interfaces;    

    public class BaseMediator: Mediator
    {
        public new const string NAME = "BaseMediator";

        protected Dictionary<string, Action<INotification>> m_NotificationHandler = null;

        public BaseMediator(): this(NAME, null)
        {

        }

        public BaseMediator(string MediatorName): this(MediatorName, null)
        {

        }

        public BaseMediator(string MediatorName, object ViewComponent): base(MediatorName, ViewComponent)
        {
            m_NotificationHandler = new Dictionary<string, Action<INotification>>();
        }

        ~BaseMediator()
        {
            if (m_NotificationHandler != null)
            {
                m_NotificationHandler.Clear();
                m_NotificationHandler = null;
            }
        }

        public override void OnRegister()
        {
            base.OnRegister();
            RegisterNotificatoinHandler();
        }

        public override void OnRemove()
        {
            base.OnRemove();
            RemoveNotificationHandler();
        }

        public override void HandleNotification(INotification notification)
        {
            base.HandleNotification(notification);

            if (m_NotificationHandler != null && notification != null && m_NotificationHandler.ContainsKey(notification.Name) && m_NotificationHandler[notification.Name] != null)
            {
                m_NotificationHandler[notification.Name](notification);
            }
        }

        protected virtual void RegisterNotificatoinHandler()
        {

        }

        protected virtual void RemoveNotificationHandler()
        {
            if (m_NotificationHandler != null)
            {
                m_NotificationHandler.Clear();
            }
        }

        public void RegisterMediator()
        {
            GameFacade.GetInstance().RegisterMediator(this);
        }

        public void RemoveMediator()
        {
            GameFacade.GetInstance().RemoveMediator(MediatorName);
        }
    }
}
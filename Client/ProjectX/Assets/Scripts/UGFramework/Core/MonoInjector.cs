namespace PureMVC.UGFramework.Core
{
    using System;
    using UnityEngine;

    public class MonoInjector: MonoBehaviour
    {
        public Action StartDelegate;
        public Action UpdateDelegate;
        public Action OnDestroyDelegate;
        public Action FixedUpdateDelegate;
        public Action LateUpdateDelegate;

        public string InjectorName = string.Empty;

        void Start()
        {
            if (StartDelegate != null)
            {
                StartDelegate();
            }
        }

        void OnDestroy()
        {
            if (OnDestroyDelegate != null)
            {
                OnDestroyDelegate();
            }

            StopAllCoroutines();
        }

        void Update()
        {
            if (UpdateDelegate != null)
            {
                UpdateDelegate();
            }
        }

        void FixedUpdate()
        {
            if (FixedUpdateDelegate != null)
            {
                FixedUpdateDelegate();
            }
        }

        void LateUpdate()
        {
            if (LateUpdateDelegate != null)
            {
                LateUpdateDelegate();
            }
        }
    }
}
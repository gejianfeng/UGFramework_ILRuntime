namespace ProjectX.Controllers
{
    using PureMVC.Interfaces;
    using PureMVC.Patterns;
    using UGFramework.Core;
    using UnityEngine;

    public class PrepareManagerCommand : SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            base.Execute(notification);

            GameObject _GameSceneManagerObject = GetManagerCarrier(GameSceneManager.NAME);
            if (_GameSceneManagerObject != null)
            {
                GameSceneManager _Mrg = new GameSceneManager(_GameSceneManagerObject);
            }

            GameObject _GuiManagerObject = GetManagerCarrier(GuiManager.NAME);
            if (_GuiManagerObject != null)
            {
                GuiManager _Mrg = new GuiManager(_GuiManagerObject);
            }

            GameObject _InputManagerObject = GetManagerCarrier(InputManager.NAME);
            if (_InputManagerObject != null)
            {
                InputManager _Mrg = new InputManager(_InputManagerObject);
            }
        }

        protected GameObject GetManagerCarrier(string ManagerName)
        {
            if (string.IsNullOrEmpty(ManagerName) || GameFacade.GetInstance().HasMediator(ManagerName))
            {
                return null;
            }

            GameObject _RootObject = new GameObject();
            _RootObject.name = ManagerName;
            _RootObject.transform.position = Vector3.zero;
            _RootObject.transform.rotation = Quaternion.identity;
            _RootObject.transform.localScale = Vector3.one;

            UnityEngine.Object.DontDestroyOnLoad(_RootObject);

            return _RootObject;
        }
    }
}
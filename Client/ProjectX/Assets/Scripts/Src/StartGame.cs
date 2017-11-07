namespace PureMVC.Project
{
    using ILRuntime.Runtime.Enviorment;
    using PureMVC.UGFramework.Core;
    using System.Reflection;

    public class StartGame : State
    {
        public new const string NAME = "StartGame";

        public StartGame() : base(NAME)
        {
            
        }

        public override void OnEnter(StateMachine FSM, params object[] Params)
        {
            base.OnEnter(FSM, Params);

            System.Type _PatchLoader = System.Type.GetType(("PureMVC.Project.PatchLoader"));

            if (_PatchLoader == null)
            {
                AppDomain _AppDomain = PatchManager.GetAppDomain();
                
                if (_AppDomain != null)
                {
                    _AppDomain.Invoke("PureMVC.Project.PatchLoader", "Launch", null, null);
                }
            }
            else
            {
                MethodInfo _Method = _PatchLoader.GetMethod("Launch", BindingFlags.Public | BindingFlags.Static, null, new System.Type[] { }, null);
                
                if (_Method != null)
                {
                    _Method.Invoke(null, null);
                }
            }
        }
    }
}

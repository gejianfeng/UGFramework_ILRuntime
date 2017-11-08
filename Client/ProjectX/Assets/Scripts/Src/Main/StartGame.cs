namespace ProjectX.Main
{
    using ILRuntime.Runtime.Enviorment;
    using UGFramework.Core;
    using System.Reflection;
    using ProjectX.Managers;

    public class StartGame : State
    {
        public new const string NAME = "StartGame";

        public StartGame() : base(NAME)
        {
            
        }

        public override void OnEnter(StateMachine FSM, params object[] Params)
        {
            base.OnEnter(FSM, Params);

            System.Type _PatchLoader = System.Type.GetType(("ProjectX.PatchLoader"));

            if (_PatchLoader == null)
            {
                AppDomain _AppDomain = GameManager.GetInstance().GetAppDomain();
                
                if (_AppDomain != null)
                {
                    _AppDomain.Invoke("ProjectX.PatchLoader", "Launch", null, null);
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

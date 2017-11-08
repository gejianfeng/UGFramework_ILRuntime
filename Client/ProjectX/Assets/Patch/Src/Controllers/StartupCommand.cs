namespace ProjectX.Controllers
{
    using PureMVC.Patterns;

    public class StartupCommand: MacroCommand
    {
        protected override void InitializeMacroCommand()
        {
            base.InitializeMacroCommand();

            AddSubCommand(typeof(PrepareControllerCommand));
            AddSubCommand(typeof(PrepareProxyCommand));
            AddSubCommand(typeof(PrepareManagerCommand));
        }
    }
}

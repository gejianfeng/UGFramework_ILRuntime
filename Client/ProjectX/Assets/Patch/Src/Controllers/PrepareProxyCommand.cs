namespace ProjectX.Controllers
{
    using PureMVC.Interfaces;
    using PureMVC.Patterns;

    public class PrepareProxyCommand: SimpleCommand
    {
        public override void Execute(INotification notification)
        {
            base.Execute(notification);
        }
    }
}

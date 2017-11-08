namespace UGFramework.Core
{
    public class GuiMediator: MonoBehaviourMediator
    {
        public new const string NAME = "GuiMonoMediator";

        public GuiMediator(): this(NAME, null, false)
        {

        }

        public GuiMediator(string MediatorName): this(MediatorName, null, false)
        {

        }

        public GuiMediator(string MediatorName, object ViewComponent, bool bAutoRegister) : base(MediatorName, ViewComponent, bAutoRegister)
        {

        }

        public virtual void InitializeMediator(string MediatorName, object ViewComponent, object Param)
        {
            if (string.IsNullOrEmpty(MediatorName) || ViewComponent == null)
            {
                return;
            }

            m_mediatorName = MediatorName;
            this.ViewComponent = ViewComponent;

            GameFacade.GetInstance().RegisterMediator(this);
        }
    }
}

namespace ProjectX
{
    using UGFramework.Core;

    class PatchLoader
    {
        public static void Launch()
        {

        }

        public static void SendNotification(string NotificationName, object Param)
        {
            if (string.IsNullOrEmpty(NotificationName))
            {
                return;
            }

            GameFacade.GetInstance().SendNotification(NotificationName, Param);
        }
    }
}

namespace CozyIsland.Utils
{
    internal class Notification
    {
        public static void Show(string message)
        {
            EasyNotification.ShowNotification(message);
        }

        public static void ShowLocal(string message)
        {
            EasyNotification.ShowLocalNotification(message);
        }
    }
}

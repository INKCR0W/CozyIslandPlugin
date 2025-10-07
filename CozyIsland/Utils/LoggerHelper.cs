namespace CozyIsland.Utils
{
    internal class LoggerHelper
    {
        public static void Info(string message) => Plugin.Log.LogInfo(message);
        public static void Warn(string message) => Plugin.Log.LogWarning(message);
        public static void Error(string message) => Plugin.Log.LogError(message);
    }
}

namespace MikManager.Util
{
    public static class Debug
    {
        public static void LogError(string message, string logger)
        {
            Console.WriteLine($"[{GetTimestamp()}] [{logger}/ERROR]: {message}");
        }

        public static void LogWarning(string message, string logger)
        {
            Console.WriteLine($"[{GetTimestamp()}] [{logger}/WARNING]: {message}");
        }

        public static void LogInfo(string message, string logger)
        {
            Console.WriteLine($"[{GetTimestamp()}] [{logger}/INFO]: {message}");
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
        }
    }
}
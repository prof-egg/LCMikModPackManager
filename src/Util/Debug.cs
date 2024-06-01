namespace MikManager.Util
{
    public static class Debug
    {
        public static void LogError(string message, string logger, string prependedItem = "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{prependedItem}[{GetTimestamp()}] [{logger}/ERROR]: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogWarning(string message, string logger, string prependedItem = "")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{prependedItem}[{GetTimestamp()}] [{logger}/WARNING]: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void LogInfo(string message, string logger, string prependedItem = "")
        {
            Console.WriteLine($"{prependedItem}[{GetTimestamp()}] [{logger}/INFO]: {message}");
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
        }
    }
}
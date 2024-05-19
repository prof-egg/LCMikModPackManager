using System;
using MikManager.Util;
using MikManager.MenuStuff;

namespace MikManager
{
    public class App
    {
        public static void Main(string[] args)
        {
            Scanner scanner = new Scanner(Console.In);
            BaseMenu.InjectScanner(scanner);
            BaseMenu.PrintLabel();

            RepoHandler.Test();
            RepoHandler.UpdateRateLimitDetails();
            // Console.WriteLine("Doing stuff...");
            Console.WriteLine();

            MenuHandler.Initialize(new HomeMenu());

            // GitHubRepoFiles.Test();
            // RepoHandler.UpdateRateLimitDetails();
        }
    }
}
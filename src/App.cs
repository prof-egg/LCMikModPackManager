using System;
using MikManager.Util;
using MikManager.MenuStuff;
using MikManager.Handlers;

namespace MikManager
{
    public class App
    {
        public static void Main(string[] args)
        {
            ThunderstoreHandler.DownloadMod("bizzlemip", "BiggerLobby", "2.7.0");
            
            // Scanner scanner = new Scanner(Console.In);
            // BaseMenu.InjectScanner(scanner);
            // BaseMenu.PrintLabel();

            // RepoHandler.Test();
            // RepoHandler.UpdateRateLimitDetails();
            // Console.WriteLine();

            // MenuHandler.Initialize(new HomeMenu());
        }
    }
}
using MikManager.Util;
using MikManager.MenuStuff;
using MikManager.MenuStuff.Menus;
using MikManager.Handlers;

namespace MikManager
{
    public class App
    {
        public static void Main(string[] args)
        {
            Scanner scanner = new Scanner(Console.In);
            BaseMenu.InjectScanner(scanner);
            BaseMenu.PrintLabel();

            RepoHandler.UpdateModDataCache();
            RepoHandler.UpdateRateLimitDetails();
            Console.WriteLine();

            MenuHandler.Initialize(new HomeMenu());
        }
    }
}
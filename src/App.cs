using MikManager.Util;
using MikManager.MenuStuff;
using MikManager.MenuStuff.Menus;
using MikManager.Handlers;
using MikManager.CustomFileTypes;

namespace MikManager
{
    public class App
    {
        public static void Main(string[] args)
        {
            // NOTE: TODO: If the user types "sex" show the admin menus as well, this would be rate limits, dependency viewing, and other stuff
            Scanner scanner = new Scanner(Console.In);
            BaseMenu.InjectScanner(scanner);
            BaseMenu.PrintLabel();

            RepoHandler.UpdateModDataCache();
            RepoHandler.UpdateRateLimitDetails();
            MikPathGuardian.EnsureMikManagerDirs();
            DependencyManager.Read();
            Console.WriteLine();

            MenuHandler.Initialize(new HomeMenu());

            // DependencyManager.AddReference("test");
            // DependencyManager.AddReference("test2");
            // DependencyManager.GetReferences("test");
            // DependencyManager.Write();
            // DependencyManager.Read();
            // Console.WriteLine(DependencyManager.GetReferences("test"));
        }
    }
}
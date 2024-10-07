using MikManager.Util;
using MikManager.MenuStuff;
using MikManager.MenuStuff.Menus;
using MikManager.Handlers;
using MikManager.CustomFileTypes;
using MikManager.Scripts;

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
            MikPathGuardian.EnsureMikManagerDirs();
            DependencyManager.Read();
            LCMDWarehouse.UpdateWarehouse();
            Console.WriteLine();

            MenuHandler.Initialize(new HomeMenu());

            // ClientMods.CheckUpdates("v64");
            // MikModDescription modd = new MikModDescription("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company\\MikModManager\\mods\\vasanex-ItemQuickSwitch-1.1.0.lcmd");
            // MikModDescription modd = new MikModDescription("C:\\Users\\BMike\\Downloads\\vasanex-ItemQuickSwitch-1.1.0", false);
            // modd.Write();
            // modd.Install();
            // modd.Delete();
            // Console.WriteLine(modd);
        }
    }
}
using MikManager.Util;
using MikManager.MenuStuff;
using MikManager.Handlers;

namespace MikManager
{
    public class App
    {
        public static void Main(string[] args)
        {
            // RepoHandler.DownloadFileFromRepo("mod-pack-data/BiggerLobby.yaml");

            // HashSet<string> paths = ThunderstoreHandler.DownloadModWithDependencies("bizzlemip", "BiggerLobby", "2.7.0");
            // ThunderstoreHandler.InstallMods(paths);
            // ThunderstoreHandler.DeleteInstalledMods();

            // ThunderstoreHandler.DownloadMod("BepInEx", "BepInExPack", "5.4.2100", true, true);

            // foreach (string path in paths)
            //     Console.WriteLine(path);

            Scanner scanner = new Scanner(Console.In);
            BaseMenu.InjectScanner(scanner);
            BaseMenu.PrintLabel();

            RepoHandler.UpdateModPackConfigList();
            RepoHandler.UpdateRateLimitDetails();
            Console.WriteLine();

            MenuHandler.Initialize(new HomeMenu());
        }
    }
}
using MikManager.CustomFileTypes;
using MikManager.Handlers;
using MikManager.Util;

namespace MikManager.MenuStuff.Menus
{
    public class ModPackListMenu : BaseMenu
    {
        private static readonly int LOWER_CHOICE_BOUND = 1;
        private const string loggerID = "ModPackListMenu";

        public override void PrintMenu()
        {                
            Console.WriteLine("MODPACK CONFIG MENU:");

            string[]? configList = RepoHandler.GetModConfigList();

            if (configList == null) 
                Console.WriteLine("Error: modpack config list was null");
            else 
            {
                // List mod pack options
                for (int i = 0; i < configList.Length; i++) 
                    Console.WriteLine($"({i + 1}) {configList[i]}");
            
                Console.WriteLine(); // For spacing between options

                Console.WriteLine($"({this.GetUpperChoiceBound() - 1}) Delete installed mods");
                Console.WriteLine($"({this.GetUpperChoiceBound()}) Go back");
            }

            if (!Directory.Exists(ModHandler.GetLCPath()))
                Debug.LogWarning($"Unable to locate lethal company. Path checked: {ModHandler.GetLCPath()}", loggerID, "\n");

            Console.Write("\nWhat pack would you like to install? ");
        }

        protected override BaseMenu? HandleInput(int selection)
        {
            // Delete installed mods choice
            if (selection == this.GetUpperChoiceBound() - 1)
            {   
                ModHandler.DeleteInstalledCautious();
                Console.WriteLine(); // for console spacing
                return this;
            }

            // Go back a page choice
            if (selection == this.GetUpperChoiceBound())
                return null;
            
            // Delete old mods
            ModHandler.DeleteInstalledCautious();

            // INSTALL MOD PACK SPECIFIED FROM YAML
            // Get modpack file name
            string[]? configList = RepoHandler.GetModConfigList();
            if (configList == null) 
            {
                Console.WriteLine("Error: modpack config list was null");
                return this;
            }
            string configFileName = configList[selection - 1];
            if (configFileName == null)
            {
                Console.WriteLine("Error: couldn't get config file name from user input");
                return this;
            }

            // Download config
            string relDownloadString = $"{RepoHandler.ModDataDirName}/{RepoHandler.ModPacksDirName}/{RepoHandler.GetGameVersion()}/{configFileName}";
            RepoHandler.DownloadFileFromRepo(relDownloadString);

            // Parse config
            string configPath = RepoHandler.GetDownloadPath(configFileName);
            Config configObj = YamlHandler.ParseModConfigFile(configPath);

            // Download and install mods
            HashSet<string> modDownloadPaths = ThunderstoreHandler.DownloadModsWithDependencies(configObj);
            DependencyManager.Write();
            ModHandler.InstallMods(modDownloadPaths);

            RepoHandler.UpdateRateLimitDetails();
            Console.WriteLine(); // For spacing

            return this;
        }

        protected override int GetUpperChoiceBound()
        {
            string[]? configList = RepoHandler.GetModConfigList();
            if (configList == null)
                // This will give the user one option, which is to back out
                return 1;
            // This will give the user one option per modpack, 
            // one extra option to back out, and another 
            // extra to delete their installed mods
            return configList.Length + 2;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }

        protected override string? GetNote()
        {
            string s1 = "Installing a modpack DELETES any currently installed mods for consistency reasons. ";
            string s2 = "This ensures there are no server-side mod inconsistencies when your group plays. ";
            string s3 = "If you wish to keep those mods, please save them somewhere else before continuing.";
            return s1 + s2 + s3;
        }
    }
}
using MikManager.Handlers;
using Newtonsoft.Json.Linq;

namespace MikManager.MenuStuff
{
    public class ModPackListMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;

        public override void PrintMenu()
        {
            Console.WriteLine("MODPACK CONFIG MENU:");

            JArray? configList = RepoHandler.GetModConfigList();

            if (configList == null) 
                Console.WriteLine("Error: modpack config list was null");
            else 
            {
                JToken[] configArray = configList.ToArray();
                for (int i = 0; i < configList.Count; i++) 
                {
                    JToken jsonObject = configArray[i];
                    string? name = jsonObject.Value<string>("name");
                    Console.WriteLine($"({i + 1}) {name}");
                }
                Console.WriteLine(); // For spacing between options
                Console.WriteLine($"({this.GetUpperChoiceBound() - 1}) Delete installed mods");
                Console.WriteLine($"({this.GetUpperChoiceBound()}) Go back");
            }
            Console.Write("\nWhat pack would you like to install? ");
        }

        protected override BaseMenu? HandleInput(int selection)
        {
            // Delete installed mods
            if (selection == this.GetUpperChoiceBound() - 1)
            {   
                ModHandler.DeleteInstalledCautious();
                Console.WriteLine(); // for console spacing
                return this;
            }

            // Go back a page
            if (selection == this.GetUpperChoiceBound())
                return null;
            
            // Delete old mods
            ModHandler.DeleteInstalledCautious();

            // INSTALL MOD PACK SPECIFIED FROM YAML
            // Get modpack file name
            JArray? configList = RepoHandler.GetModConfigList();
            if (configList == null) 
            {
                Console.WriteLine("Error: modpack config list was null");
                return this;
            }
            JToken[] configArray = configList.ToArray();
            string? configFileName = configArray[selection - 1].Value<string>("name");
            if (configFileName == null)
            {
                Console.WriteLine("Error: couldn't get config file name from user input");
                return this;
            }

            // Download config
            RepoHandler.DownloadFileFromRepo($"{RepoHandler.ModConfigPath}/{configFileName}");

            // Parse config
            string configPath = RepoHandler.GetDownloadPath(configFileName);
            Config configObj = YamlHandler.ParseModConfigFile(configPath);

            // Download and install mods
            HashSet<string> modDownloadPaths = ThunderstoreHandler.DownloadModsWithDependencies(configObj.Mods);
            ModHandler.InstallMods(modDownloadPaths);
            Console.WriteLine(); // For spacing

            return this;
        }

        protected override int GetUpperChoiceBound()
        {
            JArray? configList = RepoHandler.GetModConfigList();
            if (configList == null)
                // This will give the user one option, which is to back out
                return 1;
            // This will give the user one option per modpack, 
            // one extra option to back out, and another 
            // extra to delete their installed mods
            return configList.Count + 2;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }
    }
}
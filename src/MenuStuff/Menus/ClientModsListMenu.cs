using MikManager.Handlers;
using MikManager.Util;
using Newtonsoft.Json.Linq;

namespace MikManager.MenuStuff.Menus
{
    public class ClientModsListMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const string loggerID = "ClientModsListMenu";

        public override void PrintMenu()
        {
            Console.WriteLine("CLIENT MODS LIST MENU:");

            string[]? clientModsList = RepoHandler.GetClientModsList();

            if (clientModsList == null) 
                Debug.LogError("Client mods list was null", loggerID);
            else 
            {
                for (int i = 0; i < clientModsList.Length; i++) 
                    Console.WriteLine($"({i + 1}) {clientModsList[i]}");
            
                Console.WriteLine(); // For spacing between options
                Console.WriteLine($"({this.GetUpperChoiceBound() - 1}) Delete installed mods");
                Console.WriteLine($"({this.GetUpperChoiceBound()}) Go back");
            }
            Console.Write("\nWhat mod would you like to install? ");
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

            // INSTALL MOD
            // Get mod file name (which should also just be the dependency string)
            string[]? clientModsList = RepoHandler.GetClientModsList();
            if (clientModsList == null) 
            {
                Debug.LogError("Client mods list was null", loggerID);
                return this;
            }
            string clientModFileName = clientModsList[selection - 1];
            if (clientModFileName == null)
            {
                Debug.LogError("Couldn't get mod file name from user input", loggerID);
                return this;
            }

            // Download and install mod based on selected file name
            Mod mod = new Mod(clientModFileName);
            HashSet<string> modDownloadPaths = ThunderstoreHandler.DownloadModWithDependencies(mod);
            ModHandler.InstallMods(modDownloadPaths);

            RepoHandler.UpdateRateLimitDetails();
            Console.WriteLine(); // For spacing

            return this;
        }

        protected override int GetUpperChoiceBound()
        {
            string[]? clientModsList = RepoHandler.GetClientModsList();
            if (clientModsList == null)
                // This will give the user one option, which is to back out
                return 1;
            // This will give the user one option per modpack, 
            // one extra option to back out, and another 
            // extra to delete their installed mods
            return clientModsList.Length + 2;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }
    }
}
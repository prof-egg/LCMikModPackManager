using MikManager.Handlers;
using MikManager.Util;

namespace MikManager.MenuStuff.Menus
{
    public class PathMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const int UPPER_CHOICE_BOUND = 3;
        private const string loggerID = "PathsMenu";

        public override void PrintMenu()
        {
            Console.WriteLine("PATHS MENU:");
            Console.WriteLine("(1) Download path: " + ThunderstoreHandler.GetDownloadPath());
            Console.WriteLine("(2) Lethal Company path: " + ModHandler.GetLCPath());
            Console.WriteLine("(3) Go back");

            Console.Write("\nSelect a path would you like to change: ");
        }

        protected override BaseMenu? HandleInput(int selection)
        {
            // Go back a page
            if (selection == 3)
                return null;
            
            // Prompt for new path
            string? newPath;
            Console.Write("Please input the new path: ");
            newPath = input.NextLine();
            if (newPath == null)
            {
                Debug.LogError("Line read was null", loggerID);
                return this;
            }
            newPath = newPath.Trim();

            // If path does not exist log error and return out
            if (!Directory.Exists(newPath))
            {
                Debug.LogError("Specified path does not exist: " + newPath, loggerID);
                Console.WriteLine(); // For spacing
                return this;
            }
            
            // For spacing
            Console.WriteLine(); 
                
            // Set new path
            if (selection == 1)
                ThunderstoreHandler.SetDownloadPath(newPath);
            else if (selection == 2)
                ModHandler.SetLCPath(newPath);

            return this;
        }

        protected override int GetUpperChoiceBound()
        {
            return UPPER_CHOICE_BOUND;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }

        // protected override string? GetNote()
        // {
        //     return "Download and install path must be on the same drive.";
        // }
    }
}
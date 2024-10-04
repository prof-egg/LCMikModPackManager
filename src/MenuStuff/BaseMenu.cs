using MikManager.Util;

namespace MikManager.MenuStuff
{
    public abstract class BaseMenu
    {
        private static readonly string CLIENT_VERSION = "2.3.1";
        protected static Scanner? scanner;
        protected Scanner input;

        public BaseMenu()
        {
            if (scanner == null) 
                throw new Exception("Tried to instantiate menu without first injecting scanner dependency.");
            this.input = scanner;
        }

        /** Prints the menu, requests an input, handles the input.*/
        public BaseMenu? Open()
        {
            this.PrintMenu();
            while (true)
            {
                int input = this.GetInput();
                // if (input == -1)
                //     break;
                return this.HandleInput(input);
            }
        }

        public static void PrintLabel()
        {
            Console.WriteLine("             _                                                      ");
            Console.WriteLine("           _| |                                                     ");
            Console.WriteLine(" _ __ ___ ( ) | __  _ __ ___   __ _ _ __   __ _  __ _  ___ _ __     ");
            Console.WriteLine("| '_ ` _ \\| | |/ / | '_ ` _ \\ / _` | '_ \\ / _` |/ _` |/ _ \\ '__|");
            Console.WriteLine("| | | | | | |   <  | | | | | | (_| | | | | (_| | (_| |  __/ |       ");
            Console.WriteLine("|_| |_| |_|_|_|\\_\\ |_| |_| |_|\\__,_|_| |_|\\__,_|\\__, |\\___|_| " + $"v{CLIENT_VERSION}");
            Console.WriteLine("                                                 __/ |              ");
            Console.WriteLine("                                                |___/               ");
        }

        public void PrintNote()
        {
            string? note = GetNote();
            if (note != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("NOTE: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(note); 
            }
        }

        public bool HasNote()
        {
            return this.GetNote() != null;
        }
        /***************************************************************************
        * Helper Methods
        ***************************************************************************/

        // Can be overridden if I want to do so later
        // not sure why I would want to do that though
        private int GetInput()
        {
            while (true)
            {
                // Get input
                int select = input.NextInt();
                if (select == -1)
                    return -1;
                else if (select < this.GetLowerChoiceBound() || select > this.GetUpperChoiceBound())
                    Console.WriteLine(this.GenerateInputErrorMsg());
                else
                    return select;
            }
        }

        protected string GenerateInputErrorMsg()
        {
            return $"Oops! Please enter a valid digit between {this.GetLowerChoiceBound()} and {this.GetUpperChoiceBound()}!\n";
        }

        public static void ClearConsole()
        {
            // \033[H - Move cursor to top left
            // \033[2J - Clear entire screen
            // Read more: https://en.wikipedia.org/wiki/ANSI_escape_code
            Console.Clear();
        }

        public static void InjectScanner(Scanner newScanner)
        {
            scanner = newScanner;
        }

        /***************************************************************************
        * Abstract Declarations/Overridables
        ***************************************************************************/
        public abstract void PrintMenu();
        
        /// Returns an instance of {@code BaseMenu}. Return a new instance of a menu
        /// if you wish the {@code MenuHandler} to change pages, return {@code this} if 
        /// you wish to stay on the same page, and return {@code null} if you wish
        /// to go back a page.
        /// @param selection - The numerical input that the user gave
        /// @return an instance of {@code BaseMenu}
        protected abstract BaseMenu? HandleInput(int selection);
        protected abstract int GetUpperChoiceBound();
        protected abstract int GetLowerChoiceBound();

        protected virtual string? GetNote()
        {
            return null;
        }
    }
}
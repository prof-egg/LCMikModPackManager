namespace MikManager.MenuStuff.Menus
{
    public class HomeMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const int UPPER_CHOICE_BOUND = 6;

        public override void PrintMenu()
        {
            Console.WriteLine("HOME MENU:");
            Console.WriteLine("(1) View modpack configs");
            Console.WriteLine("(2) View client mods list");
            Console.WriteLine("(3) View installed mods");
            Console.WriteLine("(4) Change paths");
            Console.WriteLine("(5) View dependency references");
            Console.WriteLine("(6) View request data");
            Console.Write("\nWhat would you like to do? ");
        }

        protected override BaseMenu HandleInput(int selection)
        {
            switch (selection)
            {
                case 1:
                    return new ModPackListMenu();
                case 2:
                    return new ClientModsListMenu();
                case 3:
                    return new ViewModsMenu();
                case 4:
                    return new PathMenu();
                case 5:
                    return new ViewReferencesMenu();
                case 6:
                    return new RequestsMenu();
                default:
                    Console.WriteLine(); // For spacing in the console
                    return this;
            }
        }

        protected override int GetUpperChoiceBound()
        {
            return UPPER_CHOICE_BOUND;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }
    }
}
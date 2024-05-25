namespace MikManager.MenuStuff
{
    public static class MenuHandler
    {
        private static Stack<BaseMenu> menuStack = new Stack<BaseMenu>();

        public static void Initialize(BaseMenu menu)
        {
            menuStack.Push(menu);
            BaseMenu currentMenu = menuStack.Peek();
            BaseMenu? newMenu;

            while (true)
            {
                newMenu = currentMenu.Open();
                bool returnedSameInstance = newMenu == currentMenu;

                if (newMenu == null)
                    menuStack.Pop();
                else if (!returnedSameInstance)
                    menuStack.Push(newMenu);

                currentMenu = menuStack.Peek();
                if (newMenu == null || !returnedSameInstance)
                {
                    BaseMenu.ClearConsole();
                    BaseMenu.PrintLabel();
                    if (currentMenu.HasNote())
                    {
                        currentMenu.PrintNote();
                        // For spacing between note and options
                        Console.WriteLine(); 
                    }
                }
            }
        }
    }
}
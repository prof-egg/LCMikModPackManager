namespace MikManager.MenuStuff
{
    public static class MenuHandler
    {
        private static Stack<BaseMenu> menuStack = new Stack<BaseMenu>();

        public static void Initialize(BaseMenu menu)
        {
            menuStack.Push(menu);
            BaseMenu? newMenu;

            while (true)
            {
                newMenu = menuStack.Peek().Open();
                bool returnedSameInstance = newMenu == menuStack.Peek();

                if (newMenu == null)
                {
                    menuStack.Pop();
                    BaseMenu.ClearConsole();
                    BaseMenu.PrintLabel();
                }
                else if (!returnedSameInstance)
                {
                    menuStack.Push(newMenu);
                    BaseMenu.ClearConsole();
                    BaseMenu.PrintLabel();
                }
            }
        }
    }
}
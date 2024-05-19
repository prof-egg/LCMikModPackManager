using System;

namespace MikManager.MenuStuff
{
    public class RequestsMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const int UPPER_CHOICE_BOUND = 1;

        public override void PrintMenu()
        {
            Console.WriteLine("LIMIT RATES DATA:");
            Console.WriteLine("Rate Limit: " + RepoHandler.GetRateLimit());
            Console.WriteLine("Requests Remaining: " + RepoHandler.GetRequestsRemaining());
            Console.WriteLine("Limit Reset Date: " + RepoHandler.GetLimitResetDate());
            Console.WriteLine("Limit Reset Time: " + RepoHandler.GetLimitResetTime());
            Console.Write("\nType 1 to go back: ");
        }

        protected override BaseMenu? HandleInput(int selection)
        {
            return null;
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
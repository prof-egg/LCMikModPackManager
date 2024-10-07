using System.Text;
using MikManager.CustomFileTypes;
using MikManager.Handlers;

namespace MikManager.MenuStuff.Menus
{
    public class ViewReferencesMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const int UPPER_CHOICE_BOUND = 1;

        public override void PrintMenu()
        {
            Console.WriteLine("DEPENDENCY REFERENCES:");
            Console.WriteLine(DependencyManager.ToString().Replace(":", ": ") + '\n');
            Console.Write("Type 1 to go back: ");
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
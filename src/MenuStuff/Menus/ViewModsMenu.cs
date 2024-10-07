using System.Text;
using MikManager.CustomFileTypes;
using MikManager.Handlers;

namespace MikManager.MenuStuff.Menus
{
    public class ViewModsMenu : BaseMenu
    {
        private const int LOWER_CHOICE_BOUND = 1;
        private const int UPPER_CHOICE_BOUND = 1;

        public override void PrintMenu()
        {
            MikModDescription[] descriptions = LCMDWarehouse.ModDescriptions;
            StringBuilder build = new StringBuilder((descriptions.Length + 5) * 20);
            for (int i = 0; i < descriptions.Length; i++)
            {
                build.Append('(').Append(i + 1).Append(") ");
                build.Append(descriptions[i].DependencyString).Append('\n');
            }
            string s = build.ToString();
            Console.WriteLine("MODS INSTALLED:");
            Console.WriteLine(s.Length > 0 ? s : "no mods installed\n");
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
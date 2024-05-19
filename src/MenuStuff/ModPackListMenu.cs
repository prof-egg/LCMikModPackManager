using System;
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
            {
                Console.WriteLine("Error: modpack config list was null");
                
            } 
            else 
            {
                JToken[] configArray = configList.ToArray();
                for (int i = 0; i < configList.Count; i++) 
                {
                    JToken jsonObject = configArray[i];
                    string? name = jsonObject.Value<string>("name");
                    Console.WriteLine($"({i + 1}) {name}");
                }
                Console.WriteLine($"({this.GetUpperChoiceBound()}) Go back");
            }
            Console.Write("\nWhat pack would you like to install? ");
        }

        protected override BaseMenu? HandleInput(int selection)
        {
            if (selection == this.GetUpperChoiceBound())
                return null;
            return this;
        }

        protected override int GetUpperChoiceBound()
        {
            JArray? configList = RepoHandler.GetModConfigList();
            if (configList == null)
                // This will give the user one option, which is to back out
                return 1;
            // This will give the user one option per modpack, 
            // and then one extra option to back out
            return configList.Count + 1;
        }

        protected override int GetLowerChoiceBound()
        {
            return LOWER_CHOICE_BOUND;
        }
    }
}
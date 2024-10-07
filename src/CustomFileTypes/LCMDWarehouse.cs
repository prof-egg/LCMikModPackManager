using MikManager.Util;

namespace MikManager.CustomFileTypes
{
    public static class LCMDWarehouse
    {
        private static readonly string loggerID = "LCMDWarehouse";

        private static MikModDescription[] descriptions = [];

        /// <summary>
        /// Loads all .lcmd files in MikModManager/mods
        /// </summary>
        public static void UpdateWarehouse()
        {
            try {
                Debug.LogInfo("Updating warehouse...", loggerID);
                if (!MikPathGuardian.EnsureMikManagerDirsQuiet()) return;
                string[] files = Directory.GetFiles(MikPathGuardian.modFilesPath);
                descriptions = new MikModDescription[files.Length];
                for (int i = 0; i < files.Length; i++)
                    descriptions[i] = new MikModDescription(files[i]);
            } catch (Exception e) {
                Debug.LogError($"Unable to retrieve mod descriptions: {e}", loggerID);
            }
        }

        // public static void WriteAll()
        // {
        //     foreach (MikModDescription mod in descriptions)
        //         mod.Write();
        // }

        /// <summary>
        /// Calls the Delete() method on each MikModDescription object and calls LCMDWarehouse.UpdateWarehouse(); afterwords
        /// </summary>
        public static void DeleteAll()
        {
            foreach (MikModDescription mod in descriptions)
                mod.Delete();
            UpdateWarehouse();
        }
        
        public static MikModDescription? GetModDescription(string dependencyString)
        {
            for (int i = 0; i < descriptions.Length; i++)
                if (dependencyString == descriptions[i].DependencyString)
                    return descriptions[i];
            return null;
        }

        public static MikModDescription[] ModDescriptions => descriptions;
    }
}
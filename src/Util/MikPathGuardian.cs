// Ensures LethalCompany/MikModManager, LethalCompany/MikModManager/mods, and LethalCompany/MikModManager/downloads are there
using MikManager.Handlers;

namespace MikManager.Util
{
    public static class MikPathGuardian 
    {
        public static readonly string parentDir = "MikModManager";
        public static readonly string modsDirName = "mods";
        public static readonly string downloadsDirName = "downloads";
        public static readonly string downloadsPath = $"{ModHandler.GetLCPath()}\\{parentDir}\\{downloadsDirName}";
        public static readonly string modFilesPath = $"{ModHandler.GetLCPath()}\\{parentDir}\\{modsDirName}";

        private static readonly string[] directories = [parentDir, $"{parentDir}\\{modsDirName}", $"{parentDir}\\{downloadsDirName}"];
        private static readonly string loggerID = "MikPathGuardian";

        /// <summary>
        /// Calls EnsureMikManagerDirs() if DoDirsExist(out List<string> folderNames) returns false
        /// </summary>
        public static bool EnsureMikManagerDirsQuiet() => DoDirsExist() || EnsureMikManagerDirs();
        

        public static bool EnsureMikManagerDirs()
        {
            bool succesful = true;
            Debug.LogInfo("Ensuring MikManager directories...", loggerID);
            if (!Directory.Exists(ModHandler.GetLCPath()))
            {
                Debug.LogWarning($"Unable to locate lethal company. Path checked: {ModHandler.GetLCPath()}", loggerID);
                return false;
            }

            foreach (string dir in directories)
            {
                string path = $"{ModHandler.GetLCPath()}/{dir}";
                if (!Directory.Exists(path))
                {
                    try { Directory.CreateDirectory(path); }
                    catch (Exception e)
                    {
                        Debug.LogError($"Unable to create {Path.GetFileName(path)} directory: {e}", loggerID);
                        succesful = false;
                    }
                    Debug.LogInfo($"Created {Path.GetFileName(path)} directory", loggerID);
                }
            }
            return succesful;
        }

        public static bool DoDirsExist()
        {
            bool exist = true;
            foreach (string dir in directories)
            {
                string path = $"{ModHandler.GetLCPath()}/{dir}";
                if (!Directory.Exists(path))
                    exist = false;
            }
            return exist;
        }
    }
}
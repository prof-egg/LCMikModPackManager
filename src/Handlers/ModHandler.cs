using MikManager.Util;

namespace MikManager.Handlers
{
    public static class ModHandler
    {
        private const string loggerID = "ModHandler";
    
        private static readonly string DEFAULT_LC_PATH = Path.Combine("C:", "Program Files (x86)", "Steam", "steamapps", "common", "Lethal Company");
        private static string lcPath = SteamPathGuesser.GuessLCPath() ?? "null";

        public static bool InstallMods(IEnumerable<string> extractedModPaths)
        {
            if (!Directory.Exists(GetLCPath()))
            {
                Debug.LogError($"Unable to find lethal company folder. Path checked: {GetLCPath()}", loggerID);
                Debug.LogInfo("Aborting install...", loggerID);
                Debug.LogInfo($"Downloaded mods can be found in: {ThunderstoreHandler.GetDownloadPath()}", loggerID);
                return false;
            }

            foreach (string modPath in extractedModPaths)
            {
                string modName = modPath.Substring(modPath.LastIndexOf('/') + 1);
                Debug.LogInfo($"Installing {modName}...", loggerID);

                bool canInstall = DetermineSourceAndInstallPath(modPath, out string sourcePath, out string installPath);
                if (!canInstall)
                {
                    Debug.LogError($"Unable to install {modName}, manual intervention required.", loggerID);
                    Debug.LogInfo($"Mod can be found at: {modPath}", loggerID);
                    continue;
                }
                // Debug.LogInfo("Source: " + sourcePath, loggerID);
                // Debug.LogInfo("Install: " + installPath, loggerID);

                // Move content
                MoveDirectoryContents(sourcePath, installPath);
            
                // Delete whats left of original download
                // Debug.LogInfo("Deleting leftover files...", loggerID);
                Directory.Delete(modPath, true);
            }
            return true;
        }

        /// <summary>
        /// Deletes <b>known</b> mod directories and files.
        /// </summary>
        public static void DeleteInstalledCautious()
        {
            Debug.LogInfo("Deleting installed mods...", loggerID);

            string bepInExPath = GetLCPath() + "/BepInEx";
            string dissonancePath = GetLCPath() + "/Dissonance_Diagnostics";

            string doorStopPath = GetLCPath() + "/doorstop_config.ini";
            string winhttpPath = GetLCPath() + "/winhttp.dll";

            // Delete BepInEx mod folder and supporting files
            if (Directory.Exists(bepInExPath))
                Directory.Delete(bepInExPath, true);
            if (File.Exists(doorStopPath))
                File.Delete(doorStopPath);
            if (File.Exists(winhttpPath))
                File.Delete(winhttpPath);
            
            // Delete skin walker supporting folder
            if (Directory.Exists(dissonancePath))
                Directory.Delete(dissonancePath, true);
        }

        /// <summary>
        /// Deletes <b>everything</b> that isn't a known game file or directory.
        /// Method only works for the default lethal company path, as if the user
        /// accidentally sets a path that isn't the lethal company game folder, 
        /// then this method would delete everything in that folder.
        /// </summary>
        public static void DeleteInstalledDangerous()
        {
            Debug.LogInfo("Deleting installed mods...", loggerID);
            if (GetLCPath() != DEFAULT_LC_PATH)
            {
                Debug.LogError("Attempted dangerous delete routine when LC_PATH != DEFAULT_LC_PATH", loggerID);
                Debug.LogInfo("Aborting...", loggerID);
                return;
            }
                
            string lcData = "Lethal Company_Data";
            string monoBleeding = "MonoBleedingEdge";
            string[] relGameDirectories = [lcData, monoBleeding];

            string icon = "icon.png";
            string exe = "Lethal Company.exe";
            string nvngx = "nvngx_dlss.dll";
            string unityPlugin = "NVUnityPlugin.dll";
            string crashHandler = "UnityCrashHandler64.exe";
            string unityPlayer = "UnityPlayer.dll";
            string[] relGameFiles = [icon, exe, nvngx, unityPlugin, crashHandler, unityPlayer];

            string[] allDirectoryPaths = Directory.GetDirectories(GetLCPath());
            foreach (string path in allDirectoryPaths)
            {
                if (!relGameDirectories.Contains(Path.GetFileName(path)))
                    Directory.Delete(path, true);
                    // Debug.LogInfo($"Would delete folder: {path} ({Path.GetFileName(path)})", loggerID);
            }

            string[] allFilePaths = Directory.GetFiles(GetLCPath());
            foreach (string path in allFilePaths)
            {
                if (!relGameFiles.Contains(Path.GetFileName(path)))
                    File.Delete(path);
                    // Debug.LogInfo($"Would delete file: {path}", loggerID);
            }
        }

        /***************************************************************************
        * Getters and setters
        ***************************************************************************/
        public static string GetLCPath()
        {
            return lcPath;
        }
        public static void SetLCPath(string newPath)
        {
            lcPath = newPath;
        }

        /***************************************************************************
        * Helper Methods
        ***************************************************************************/
        private static void MoveDirectoryContents(string sourceDir, string destinationDir)
        {
            // Ensure the destination directory exists
            Directory.CreateDirectory(destinationDir);

            // Move files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationDir, fileName);

                // This is specifically for skipping unnecessary content that
                // comes with the downloaded mod
                bool skip = fileName == "manifest.json" || fileName == "CHANGELOG.md" || fileName == "README.md" || fileName == "icon.png";
                if (skip) continue;
                
                try 
                {
                    if (!File.Exists(destFile))
                        File.Move(file, destFile);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error moving files and directories: {ex.Message}", loggerID);
                }
            }

            // Move directories
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(destinationDir, dirName);

                // Recursively move the contents of the directory
                MoveDirectoryContents(directory, destDir);

                // Delete the source directory after its contents have been moved
                // Directory.Delete(directory, true);
            }
        }      
        private static bool DetermineSourceAndInstallPath(string extractedModPath, out string sourcePath, out string installPath)
        {
            if (Directory.Exists($"{extractedModPath}/BepInExPack"))
            {
                sourcePath = extractedModPath + "/BepInExPack";
                installPath = GetLCPath();
                return true;
            } 
            else if (Directory.Exists($"{extractedModPath}/BepInEx"))
            {
                sourcePath = extractedModPath;
                installPath = GetLCPath();
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}/plugins"))
            {
                sourcePath = extractedModPath;
                installPath = GetLCPath() + "/BepInEx";
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}/LethalCompanyInputUtils"))
            {
                sourcePath = extractedModPath + "/LethalCompanyInputUtils";
                installPath = GetLCPath() + "/BepInEx/plugins";
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}/config") || Directory.Exists($"{extractedModPath}/patchers"))
            {
                sourcePath = extractedModPath;
                installPath = GetLCPath() + "/BepInEx";
                return true;
            }
            // Check if dll file is just in the extracted folder bare bones
            else if (Directory.GetFiles(extractedModPath).Any((filePath) => filePath.EndsWith(".dll")))
            {
                sourcePath = extractedModPath;
                installPath = GetLCPath() + "/BepInEx/plugins";
                return true;
            }
            
            // Couldn't figure out the layout, return false
            installPath = "";
            sourcePath = "";
            return false;
        }
    }
}
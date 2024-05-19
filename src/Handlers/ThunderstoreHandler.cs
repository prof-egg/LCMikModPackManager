using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAPICodePack.Shell;
using MikManager.Util;
using Newtonsoft.Json.Linq;

namespace MikManager.Handlers
{
    public static class ThunderstoreHandler
    {
        private const string loggerID = "ThunderstoreHandler";
        private static readonly HttpClient httpClient = new HttpClient();

        // This string gets prepended to the path returned by GetDownloadPath()
        private static readonly string PREPENEDED_DOWNLOAD_PATH = KnownFolders.Downloads.Path + "/";
        // private static readonly string USER_HOME = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static readonly string DEFAULT_LC_PATH = Path.Combine("C:", "Program Files (x86)", "Steam", "steamapps", "common", "Lethal Company");
        //C:\Program Files (x86)\Steam\steamapps\common\Lethal Company

        public static HashSet<string> DownloadModWithDependencies(string modDeveloper, string modId, string modVersion)
        {   
            string downloadPath = GetDownloadPath(modId, modVersion);
            string manifestJsonPath = downloadPath + "/manifest.json";

            HashSet<string> pathSet = [downloadPath];

            // Download, extract, and delete zip for mod
            bool succesfulDownload = DownloadMod(modDeveloper, modId, modVersion, true, true);
            // Errors should be logged by the DownloadMod() method
            if (!succesfulDownload) return pathSet;
                
            try {
                // Read manifest.json and get dependency string array
                string jsonString = File.ReadAllText(manifestJsonPath);
                JObject jsonObject = JObject.Parse(jsonString);
                JArray? dependencyList = (JArray?)jsonObject["dependencies"] 
                ?? throw new Exception("\"dependencies\" attribute does not exist on manifest.json");
                JToken[] dependencyArray = dependencyList.ToArray();

                // Recursively download dependency mods
                foreach (string? dependencyString in dependencyArray.Select(v => (string?)v))
                {
                    if (dependencyString == null) 
                        continue;
                    Console.WriteLine($"{modId} dependency found: {dependencyString}");

                    // Extract mod details
                    // Example dependency string: BepInEx-BepInExPack-5.4.2100
                    // modDeveloper-modId-modVersion
                    string[] modData = dependencyString.Split("-");
                    string dependencyModDev = modData[0];
                    string dependencyModId = modData[1];
                    string dependencyModVer = modData[2];

                    // Download dependency and add to downloaded paths
                    HashSet<string> newPathSet = DownloadModWithDependencies(dependencyModDev, dependencyModId, dependencyModVer);
                    pathSet.UnionWith(newPathSet);
                }
            } 
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IOException || ex is FileNotFoundException)
            {
                Debug.LogError($"Error reading manifest json: {ex.Message}", loggerID);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading dependencies: {ex.Message}", loggerID);
            }
            return pathSet;
        }

        public static bool DownloadMod(string modDeveloper, string modId, string modVersion, bool extract = false, bool deleteZip = false)
        {
            string dependencyString = $"{modDeveloper}-{modId}-{modVersion}";
            string downloadUrl = GetDownloadUrl(modDeveloper, modId, modVersion);
            string zipDownloadPath = GetZipDownloadPath(modId, modVersion);
            string downloadPath = GetDownloadPath(modId, modVersion);
            string zipFileName = $"{modId}-{modVersion}.zip";

            try
            {
                // Download zip folder
                if (File.Exists(GetZipDownloadPath(modId, modVersion)))
                    Debug.LogInfo($"{dependencyString} is already download, skipping...", loggerID);
                else
                {
                    Debug.LogInfo($"Downloading {zipFileName}...", loggerID);
                    DownloadFile(downloadUrl, zipDownloadPath);
                }

                // Extract zip folder
                if (extract)
                {
                    if (Directory.Exists(GetDownloadPath(modId, modVersion)))
                        Debug.LogInfo($"{dependencyString} is already extracted, skipping...", loggerID);
                    else
                    {
                        Debug.LogInfo($"Extracting {zipFileName}...", loggerID);
                        ZipFile.ExtractToDirectory(zipDownloadPath, downloadPath);
                    }
                }

                // Delete zip folder
                if (deleteZip)
                {
                    Debug.LogInfo($"Deleting {zipFileName}...", loggerID);
                    File.Delete(zipDownloadPath); 
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error downloading mod: {ex.Message}", loggerID);
                return false;
            }
        }

        public static void InstallMods(IEnumerable<string> extractedModPaths)
        {
            foreach (string modPath in extractedModPaths)
            {
                string sourcePath = modPath;
                string modName = modPath.Substring(modPath.LastIndexOf('/') + 1);
                Debug.LogInfo($"Installing {modName}...", loggerID);

                // Attempt to resolve BepInEx not being the first
                // directory in the mod path
                while (!Directory.Exists($"{sourcePath}/BepInEx"))
                {
                    string[] subPaths = Directory.GetDirectories(sourcePath);
                    if (subPaths.Length == 0)
                    {
                        Debug.LogError($"Failed to install {modName}", loggerID);
                        break;
                    }
                    sourcePath = subPaths[0];
                }

                // Move content
                MoveDirectoryContents(sourcePath, DEFAULT_LC_PATH);

                // Delete whats left of original download
                // Debug.LogInfo("Deleting leftover files...", loggerID);
                Directory.Delete(modPath, true);
            }
        }

        public static void DeleteInstalledMods()
        {
            Debug.LogInfo("Deleting installed mods...", loggerID);

            string doorStopPath = DEFAULT_LC_PATH + "/doorstop_config.ini";
            string winhttpPath = DEFAULT_LC_PATH + "/winhttp.dll";
            string bepInExPath = DEFAULT_LC_PATH + "/BepInEx";

            // Handle special cases
            if (File.Exists(doorStopPath))
                File.Delete(doorStopPath);
            if (File.Exists(winhttpPath))
                File.Delete(winhttpPath);

            // Delete BepInEx mod folder
            if (Directory.Exists(bepInExPath))
                Directory.Delete(bepInExPath, true);
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

        private static void DownloadFile(string url, string outputPath)
        {
            using HttpResponseMessage response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, fileBytes);
        }

        public static string GetDownloadUrl(string modDeveloper, string modId, string modVersion)
        {
            return $"https://thunderstore.io/package/download/{modDeveloper}/{modId}/{modVersion}";
        }

        public static string GetZipDownloadPath(string modId, string modVersion) 
        {
            return GetDownloadPath(modId, modVersion) + ".zip";
        }

        public static string GetDownloadPath(string modId, string modVersion) 
        {
            return PREPENEDED_DOWNLOAD_PATH + $"{modId}-{modVersion}";
        }
    }
}
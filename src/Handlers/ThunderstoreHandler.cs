using System.IO.Compression;
using MikManager.Util;
using Newtonsoft.Json.Linq;

namespace MikManager.Handlers
{
    public static class ThunderstoreHandler
    {
        private const string loggerID = "ThunderstoreHandler";
        private static readonly HttpClient httpClient = new HttpClient();

        // This string gets prepended to the path returned by GetModDownloadPath()
        private static readonly string USER_PROFILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string downloadPath = Path.Combine(USER_PROFILE_PATH, "Downloads") + "/";

        public static HashSet<string> DownloadModsWithDependencies(Config config)
        {
            HashSet<string> modDownloadPaths = [];
            foreach (string dependencyString in config.Mods)
                modDownloadPaths.UnionWith(DownloadModWithDependencies(Config.ParseDependencyString(dependencyString)));
            return modDownloadPaths;
        }

        public static HashSet<string> DownloadModWithDependencies(Mod mod)
        {   
            return  DownloadModWithDependencies(mod.Developer, mod.Id, mod.Version);
        }

        public static HashSet<string> DownloadModWithDependencies(string modDeveloper, string modId, string modVersion)
        {   
            string modDownloadPath = GetModDownloadPath(modId, modVersion);
            string manifestJsonPath = modDownloadPath + "/manifest.json";

            HashSet<string> pathSet = [modDownloadPath];

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
            string modDownloadPath = GetModDownloadPath(modId, modVersion);
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
                    if (Directory.Exists(GetModDownloadPath(modId, modVersion)))
                        Debug.LogInfo($"{dependencyString} is already extracted, skipping...", loggerID);
                    else
                    {
                        Debug.LogInfo($"Extracting {zipFileName}...", loggerID);
                        ZipFile.ExtractToDirectory(zipDownloadPath, modDownloadPath);
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

        /***************************************************************************
        * Getters and setters
        ***************************************************************************/
        public static string GetDownloadPath()
        {
            return downloadPath;
        }
        public static void SetDownloadPath(string newPath)
        {
            downloadPath = newPath;
        }

        /***************************************************************************
        * Helper Methods
        ***************************************************************************/
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
            return GetModDownloadPath(modId, modVersion) + ".zip";
        }

        public static string GetModDownloadPath(string modId, string modVersion) 
        {
            return GetDownloadPath() + $"{modId}-{modVersion}";
        }
    }
}
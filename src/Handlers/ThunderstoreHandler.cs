using System.IO.Compression;
using MikManager.CustomFileTypes;
using MikManager.Util;
using Newtonsoft.Json.Linq;

namespace MikManager.Handlers
{
    public static class ThunderstoreHandler
    {
        private const string loggerID = "ThunderstoreHandler";
        private static readonly HttpClient httpClient = new HttpClient();

        // This string gets prepended to the path returned by GetModDownloadPath()
        // private static readonly string USER_PROFILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // private static string downloadPath = Path.Combine(USER_PROFILE_PATH, "Downloads");
        private static string downloadPath = MikPathGuardian.downloadsPath;

        static ThunderstoreHandler()
        {
            httpClient.Timeout = TimeSpan.FromMinutes(5); // or Timeout.InfiniteTimeSpan to disabled
        }

        public static HashSet<string> DownloadModsWithDependencies(Config config)
        {
            HashSet<string> modDownloadPaths = [];
            foreach (string dependencyString in config.Mods)
                modDownloadPaths.UnionWith(DownloadModWithDependencies(Config.ParseDependencyString(dependencyString)));
            return modDownloadPaths;
        }

        public static HashSet<string> DownloadModWithDependencies(Mod mod)
        {   
            return DownloadModWithDependencies(mod.Developer, mod.Id, mod.Version);
        }

        public static HashSet<string> DownloadModWithDependencies(string modDeveloper, string modId, string modVersion)
        {   
            string modDownloadPath = GetModDownloadPath(modId, modVersion);
            string manifestJsonPath = modDownloadPath + "/manifest.json";

            HashSet<string> pathSet = [modDownloadPath];

            // Download, extract, and delete zip for mod
            bool succesfulDownload = DownloadMod(modDeveloper, modId, modVersion, true, true);
            // Errors should be logged by the DownloadMod() method
            if (!succesfulDownload) return new HashSet<string>();
                
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
                    DependencyManager.AddReference(dependencyString);
                    if (DependencyManager.GetReferences(dependencyString) > 1) 
                    {
                        Debug.LogInfo($"\"{dependencyString}\" already exists, skipping download...", loggerID);
                        continue;
                    }

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
                Debug.LogError($"Error reading manifest json: {ex}", loggerID);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading dependencies: {ex}", loggerID);
            }
            return pathSet;
        }

        // Marked private as to not mess up dependency manager
        private static bool DownloadMod(string modDeveloper, string modId, string modVersion, bool extract = false, bool deleteZip = false)
        {
            string dependencyString = $"{modDeveloper}-{modId}-{modVersion}";
            string downloadUrl = GetDownloadUrl(modDeveloper, modId, modVersion);
            string zipDownloadPath = GetZipDownloadPath(modId, modVersion);
            string modDownloadPath = GetModDownloadPath(modId, modVersion);
            string zipFileName = $"{modId}-{modVersion}.zip";

            if (!MikPathGuardian.EnsureMikManagerDirsQuiet())
            {
                Debug.LogError($"Unable to download \"{dependencyString}\": Lethal company folder does not exist", loggerID);
                return false;
            }

            try
            {
                // Download zip folder
                if (File.Exists(GetZipDownloadPath(modId, modVersion)))
                    Debug.LogInfo($"{dependencyString} is already download, skipping...", loggerID);
                else
                {
                    Debug.LogInfo($"Downloading {zipFileName}...", loggerID);
                    bool successfulDownload = DownloadFile(downloadUrl, zipDownloadPath);
                    if (!successfulDownload) return false;
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
                Debug.LogError($"Error downloading mod: {ex}", loggerID);
                return false;
            }
        }

        /***************************************************************************
        * Getters and setters
        ***************************************************************************/
        public static string GetDownloadPath()
        {
            return downloadPath.Replace("/", "\\");
        }
        public static void SetDownloadPath(string newPath)
        {
            downloadPath = newPath;
        }

        /***************************************************************************
        * Helper Methods
        ***************************************************************************/
        private static bool DownloadFile(string url, string outputPath)
        {
            try {
                using HttpResponseMessage response = httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                File.WriteAllBytes(outputPath, fileBytes); 
                return true;
            } catch (TaskCanceledException e) {
                Debug.LogError($"Error downloading mod, this was likely due to the download taking too long: {e}", loggerID);
            } catch (Exception e) {
                Debug.LogError($"Error downloading mod: {e}", loggerID);
            }
            return false;
        }

        // Async code by chat gpt
        // private static async Task<bool> DownloadFileAsync(string url, string outputPath, IProgress<double> progress = null)
        // {
        //     try
        //     {
        //         using HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead);
        //         response.EnsureSuccessStatusCode();

        //         using Stream contentStream = await response.Content.ReadAsStreamAsync();
        //         using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        //         byte[] buffer = new byte[8192];
        //         long totalBytes = response.Content.Headers.ContentLength ?? -1;
        //         long totalRead = 0;
        //         int readBytes;

        //         var stopwatch = new System.Diagnostics.Stopwatch();
        //         stopwatch.Start();

        //         while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //         {
        //             await fileStream.WriteAsync(buffer, 0, readBytes);
        //             totalRead += readBytes;

        //             // Report progress
        //             if (totalBytes > 0 && progress != null)
        //             {
        //                 double percentComplete = (double)totalRead / totalBytes * 100;
        //                 progress.Report(percentComplete);
        //             }

        //             // If download is taking longer than 5 seconds, start sending progress updates
        //             if (stopwatch.Elapsed.TotalSeconds > 5 && progress != null)
        //             {
        //                 double percentComplete = totalBytes > 0 ? (double)totalRead / totalBytes * 100 : -1;
        //                 progress.Report(percentComplete);
        //             }
        //         }

        //         stopwatch.Stop();
        //         return true;
        //     }
        //     catch (TaskCanceledException e)
        //     {
        //         Debug.LogError($"Error downloading mod, this was likely due to the download taking too long: {e}", loggerID);
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"Error downloading mod: {e}", loggerID);
        //     }
        //     return false;
        // }


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
            return GetDownloadPath() + $"/{modId}-{modVersion}";
        }
    }
}
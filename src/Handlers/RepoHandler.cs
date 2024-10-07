using Newtonsoft.Json.Linq;
using MikManager.Util;

namespace MikManager.Handlers
{
    public static class RepoHandler
    {
        private const string loggerID = "RepoHandler";
        private static readonly HttpClient httpClient = new HttpClient();

        private const string RepoOwner = "prof-egg";
        private const string RepoName = "LCMikModPackManager";
        private const string GitHubApiUrl = "https://api.github.com";
        private const string GitHubRateLimitApiUrl = "https://api.github.com/rate_limit";

        public const string ModDataDirName = "mod-data";
        public const string ClientModsDirName = "client-mods";
        public const string GameVersionDirName = "game-version";
        public const string ModPacksDirName = "mod-packs";
        
        // This string gets prepended to the path returned by GetDownloadPath()
        // private static readonly string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // private static readonly string PREPENEDED_DOWNLOAD_PATH = Path.Combine(userProfile, "Downloads") + "/";
        private static string PREPENEDED_DOWNLOAD_PATH = MikPathGuardian.downloadsPath + "/";

        private static int rateLimit = -1;
        private static int requestsRemaining = -1;
        private static string limitResetDate = "null";
        private static string limitResetTime = "null";

        private static string[]? modpackConfigList = null;
        private static string[]? clientModsList = [];
        private static string? gameVersion = null;

        public static void UpdateRateLimitDetails()
        {
            Debug.LogInfo("Updating request details...", loggerID);

            var request = new HttpRequestMessage(HttpMethod.Get, GitHubRateLimitApiUrl);
            request.Headers.Add("User-Agent", "HttpClient");

            try
            {
                HttpResponseMessage response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                JObject jsonObject = JObject.Parse(responseBody);

                JObject? rate = (JObject?)jsonObject["rate"];

                if (rate == null) {
                    Debug.LogError("HTTP Request failed", loggerID);
                    return;
                }
                    
                int limit = rate.Value<int>("limit");
                int remaining = rate.Value<int>("remaining");
                long reset = rate.Value<long>("reset");

                // Convert reset time to human-readable format in the user's timezone
                DateTimeOffset resetDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(reset);
                DateTime resetDate = resetDateTimeOffset.DateTime;

                // Create a DateTime format strings
                string datePattern = "ddd MMMM dd yyyy";
                string timePattern = "hh:mm:ss tt zzz";

                // Update data
                rateLimit = limit;
                requestsRemaining = remaining;
                limitResetDate = resetDate.ToString(datePattern);
                limitResetTime = resetDate.ToString(timePattern);
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Request exception: {e}", loggerID);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e}", loggerID);
            }
        }

        public static void UpdateModDataCache()
        {
            Debug.LogInfo("Updating version cache...", loggerID);
            UpdateVersionCache();
            
            Debug.LogInfo("Updating modpack config cache...", loggerID);
            UpdateModpackOrClientModsCash(true);

            Debug.LogInfo("Updating client mods cache...", loggerID);
            UpdateModpackOrClientModsCash(false);
        }

        public static void DownloadFileFromRepo(string repoFilePath)
        {
            string url = GetGitHubRepoDownloadPath(repoFilePath);
            string fileName = repoFilePath.Substring(repoFilePath.LastIndexOf('/') + 1);
            string downloadPath = GetDownloadPath(fileName);

            if (File.Exists(downloadPath))
            {
                Debug.LogInfo($"Deleting old {fileName}...", loggerID);
                File.Delete(downloadPath);
            }

            Debug.LogInfo($"Downloading {fileName}...", loggerID);
            try   { DownloadFile(url, GetDownloadPath(fileName)); }
            catch (Exception ex)
            { Debug.LogError($"Unable to download from {url}: {ex}", loggerID); }
        }

        /***************************************************************************
        * Getters
        ***************************************************************************/
        public static int GetRateLimit()
        {
            return rateLimit;
        }

        public static int GetRequestsRemaining()
        {
            return requestsRemaining;
        }

        public static string GetLimitResetDate()
        {
            return limitResetDate;
        }

        public static string GetLimitResetTime()
        {
            return limitResetTime;
        }

        public static string[]? GetModConfigList() 
        {
            return modpackConfigList;
        }

        public static string[]? GetClientModsList() 
        {
            return clientModsList;
        }

        public static string? GetGameVersion()
        {
            return gameVersion;
        }

        /***************************************************************************
        * Helper Methods
        ***************************************************************************/
        private static void UpdateVersionCache()
        {
            string url = GetGitHubAPIGameVersionURL();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "HttpClient");
            try
            {
                HttpResponseMessage response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JArray jsonArray = JArray.Parse(responseBody);
                gameVersion = jsonArray.First().Value<string>("name");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update version cache: {e}", loggerID);
            }
        }

        private static void UpdateModpackOrClientModsCash(bool updateModPackCache)
        {
            string cacheString = (updateModPackCache) ? "modpack" : "client mods";
            if (gameVersion == null)
            {
                Debug.LogError($"Failed to update {cacheString} cache: gameVersion was null", loggerID);
                return;
            }

            string url = (updateModPackCache) ? GetGitHubAPIModPacksURL(gameVersion) : GetGitHubAPIClientModsURL(gameVersion);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "HttpClient");
            try
            {
                HttpResponseMessage response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                JArray jsonArray = JArray.Parse(responseBody);

                string[] cache = jsonArray
                    .Where(token => token["name"] != null)
                    // An item should never appear "null" because of the where statement above
                    .Select(token => token.Value<string>("name") ?? "null") 
                    .ToArray();
                
                if (updateModPackCache) modpackConfigList = cache;
                else clientModsList = cache;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update {cacheString} cache: {e}", loggerID);
            }
        }

        private static void DownloadFile(string url, string outputPath)
        {
            using HttpResponseMessage response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, fileBytes);
        }

        /***************************************************************************
        * Helper url/path methods
        ***************************************************************************/
        private static string GetGitHubAPIGameVersionURL()
        {   
            return GetGitHubAPIRepoURL($"{ModDataDirName}/{GameVersionDirName}");
        }

        private static string GetGitHubAPIClientModsURL(string lcVersion)
        {   
            return GetGitHubAPIRepoURL($"{ModDataDirName}/{ClientModsDirName}/{lcVersion}");
        }

        private static string GetGitHubAPIModPacksURL(string lcVersion)
        {   
            return GetGitHubAPIRepoURL($"{ModDataDirName}/{ModPacksDirName}/{lcVersion}");
        }

        private static string GetGitHubAPIRepoURL(string relativePath)
        {
            return $"{GitHubApiUrl}/repos/{RepoOwner}/{RepoName}/contents/{relativePath}";
        }
        
        private static string GetGitHubRepoDownloadPath(string relativePath)
        {
            return $"https://raw.githubusercontent.com/{RepoOwner}/{RepoName}/main/{relativePath}";
        }

        public static string GetDownloadPath(string file) 
        {
            return PREPENEDED_DOWNLOAD_PATH + file;
        }
    }
}
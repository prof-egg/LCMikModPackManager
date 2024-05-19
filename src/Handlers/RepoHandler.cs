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
        private const string ModConfigPath = "mod-pack-data"; // Specify the folder path
        private static readonly string PREPENDED_DOWNLOAD_PATH = "downloads/";

        private static int rateLimit = -1;
        private static int requestsRemaining = -1;
        private static string limitResetDate = "null";
        private static string limitResetTime = "null";

        private static JArray? jsonConfigList = null;

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
                Console.WriteLine($"Request exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }

        public static void UpdateModPackConfigList()
        {
            Debug.LogInfo("Requesting mod pack configs...", loggerID);

            string url = GetGitHubRepoAPIPath(ModConfigPath);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "HttpClient");

            try
            {
                HttpResponseMessage response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                JArray jsonArray = JArray.Parse(responseBody);

                jsonConfigList = jsonArray;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
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
            DownloadFile(url, GetDownloadPath(fileName));
        }

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

        public static JArray? GetModConfigList() 
        {
            return jsonConfigList;
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

        private static string GetGitHubRepoAPIPath(string relativePath)
        {
            return $"{GitHubApiUrl}/repos/{RepoOwner}/{RepoName}/contents/{relativePath}";
        }
        
        private static string GetGitHubRepoDownloadPath(string relativePath)
        {
            //https://raw.githubusercontent.com/{owner}/{repo}/main/{filePath}
            return $"https://raw.githubusercontent.com/{RepoOwner}/{RepoName}/main/{relativePath}";
        }

        private static string GetDownloadPath(string file) 
        {
            return PREPENDED_DOWNLOAD_PATH + file;
        }
    }
}
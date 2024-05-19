using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using MikManager.Util;

namespace MikManager
{
    public static class RepoHandler
    {
        private const string loggerID = "RepoHandler";
        private static readonly HttpClient HttpClient = new HttpClient();

        private const string RepoOwner = "prof-egg";
        private const string RepoName = "LCMikModPackManager";
        private const string GitHubApiUrl = "https://api.github.com";
        private const string GitHubRateLimitApiUrl = "https://api.github.com/rate_limit";
        private const string FolderPath = "mod-pack-data"; // Specify the folder path

        private static int rateLimit = -1;
        private static int requestsRemaining = -1;
        private static string limitResetDate = "null";
        private static string limitResetTime = "null";

        public static void UpdateRateLimitDetails()
        {
            Debug.LogInfo("Updating request details...", loggerID);

            var request = new HttpRequestMessage(HttpMethod.Get, GitHubRateLimitApiUrl);
            request.Headers.Add("User-Agent", "HttpClient");

            try
            {
                HttpResponseMessage response = HttpClient.Send(request);
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

        public static void Test()
        {
            Debug.LogInfo("Requesting mod pack configs...", loggerID);

            string url = $"{GitHubApiUrl}/repos/{RepoOwner}/{RepoName}/contents/{FolderPath}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "HttpClient");

            try
            {
                HttpResponseMessage response = HttpClient.Send(request);
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                JArray jsonArray = JArray.Parse(responseBody);

                foreach (JObject jsonObject in jsonArray.Cast<JObject>())
                {
                    string? fileName = jsonObject.Value<string>("name");
                    string? filePath = jsonObject.Value<string>("path");
                    // Console.WriteLine($"File: {fileName} Path: {filePath}");
                }
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
    }
}
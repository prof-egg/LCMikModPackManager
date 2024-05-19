using MikManager.Util;

namespace MikManager.Handlers
{
    public static class ThunderstoreHandler
    {
        private const string loggerID = "ThunderstoreHandler";
        private static readonly HttpClient httpClient = new HttpClient();

        public static bool DownloadMod(string modDeveloper, string modId, string modVersion)
        {
            string downloadUrl = $"https://thunderstore.io/package/download/{modDeveloper}/{modId}/{modVersion}";
            string downloadPath = $"BiggerLobby-{modVersion}.zip"; // Specify the download path

            try
            {
                Debug.LogInfo("Starting download...", loggerID);
                DownloadFile(downloadUrl, downloadPath);
                Debug.LogInfo("Download complete", loggerID);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error downloading mod: {ex.Message}", loggerID);
            }

            return true;
        }

        private static void DownloadFile(string url, string outputPath)
        {
            using HttpResponseMessage response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(outputPath, fileBytes);
        }
    }
}
using Microsoft.Win32;

namespace MikManager.Util
{
    class SteamPathGuesser
    {
        private const string loggerID = "ModHandler";
        private static readonly string LC_STEAM_GAME_ID = "1966720";

        public static string? GuessLCPath()
        {
            string? steamPath = GetSteamPath();
            if (string.IsNullOrEmpty(steamPath))
                return null;

            string? gameInstallPath = FindGameInstallPath(steamPath, LC_STEAM_GAME_ID);
            if (string.IsNullOrEmpty(gameInstallPath))
                return null;

            return gameInstallPath.Replace("/", "\\");
        }

        public static string? GetSteamPath()
        {
            try
            {
                // Try to find Steam installation path from the Windows Registry
#pragma warning disable CA1416 // Validate platform compatibility
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
#pragma warning restore CA1416 // Validate platform compatibility

                if (key == null) return null;

#pragma warning disable CA1416 // Validate platform compatibility
                Object? o = key.GetValue("SteamPath");
#pragma warning restore CA1416 // Validate platform compatibility

                if (o != null)
                    return o as string;

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string? FindGameInstallPath(string steamPath, string appId)
        {
            // Console.WriteLine("Getting library paths...");
            string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
                return null;

            List<string> libraryPaths = [Path.Combine(steamPath, "steamapps")]; // Add default library path

            // Read additional library paths from libraryfolders.vdf
            foreach (var line in File.ReadLines(libraryFoldersPath))
            {
                // Console.WriteLine($"Line: {line}");
                if (line.Trim().StartsWith("\"path\""))
                {
                    var parts = line.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                        // parts[^1] == parts[parts.Length-1]
                        libraryPaths.Add(Path.Combine(parts[^1].Trim(), "steamapps"));
                }
            }


            // Console.WriteLine("LIB PATHS:");
            foreach (string path in libraryPaths)
                // Console.WriteLine(path);

            // Console.WriteLine("\nLooking for game in libarary paths...\n");

            // Search for the game in all library paths
            foreach (var libraryPath in libraryPaths)
            {
                string appManifestPath = Path.Combine(libraryPath, $"appmanifest_{appId}.acf");

                // Console.WriteLine($"Library: {libraryPath} | Exists: {Directory.Exists(libraryPath)}");
                // Console.WriteLine($"App manfiest: {appManifestPath} | Exists: {File.Exists(appManifestPath)}\n");

                if (File.Exists(appManifestPath))
                {
                    string? installDir = GetInstallDirFromAppManifest(appManifestPath);
                    if (!string.IsNullOrEmpty(installDir))
                        return Path.Combine(libraryPath, "common", installDir);
                }
            }
            return null;
        }

        public static string? GetInstallDirFromAppManifest(string appManifestPath)
        {
            foreach (var line in File.ReadLines(appManifestPath))
            {
                // Console.WriteLine($"Line: {line}");
                if (line.Trim().StartsWith("\"installdir\""))
                {
                    // Console.WriteLine("Found installdir");
                    var parts = line.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                        // parts[^1] == parts[parts.Length-1]
                        return parts[^1].Trim();
                }
            }
            return null;
        }
    }
}
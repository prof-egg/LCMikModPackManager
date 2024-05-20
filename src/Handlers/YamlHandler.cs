using MikManager.Util;
using YamlDotNet.Serialization;

namespace MikManager.Handlers
{
    public static class YamlHandler
    {
        private static readonly string loggerID = "YamlHandler";

        private static readonly Deserializer deserializer = new Deserializer();
 
        public static Config ParseModConfigFile(string modConfigPath)
        {
            Debug.LogInfo("Parsing config file...", loggerID);
            using var reader = new StreamReader(modConfigPath);
            var config = deserializer.Deserialize<Config>(reader);
            return config;
        }
    }

    public class Config {
        public required string LethalCompanyVersion { get; set; }
        public required Mod[] Mods { get; set; }
    }

    public class Mod
    {
        public required string Developer { get; set; }
        public required string Name { get; set; }
        public required string Version { get; set; }
    }
}
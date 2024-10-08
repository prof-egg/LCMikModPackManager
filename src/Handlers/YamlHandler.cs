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
            try 
            {
                Debug.LogInfo("Parsing config file...", loggerID);
                using var reader = new StreamReader(modConfigPath);
                var config = deserializer.Deserialize<Config>(reader);
                return config;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse config file: {ex}", loggerID);
                return new Config(){LethalCompanyVersion = "0", Mods = []};
            }
        }
    }

    public class Config 
    {
        public required string LethalCompanyVersion { get; set; }
        public required List<string> Mods { get; set; }
        public static Mod ParseDependencyString(string depependencyString) 
        {
            return new Mod(depependencyString);
        }
    }

    public class Mod
    {
        public readonly string Developer;
        public readonly string Id;
        public readonly string Version;
        public Mod(string depependencyString)
        {
            string[] data = depependencyString.Split('-');
            this.Developer = data[0];
            this.Id = data[1];
            this.Version = data[2];
        }
    }
}
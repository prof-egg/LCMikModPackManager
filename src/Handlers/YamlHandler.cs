using MikManager.Util;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MikManager.Handlers
{
    public static class YamlHandler
    {
        private static readonly string loggerID = "YamlHandler";

        private static readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static Config ParseModConfigFile(string modConfigPath)
        {
            Debug.LogInfo("Parsing config file...", loggerID);
            using var reader = new StreamReader(modConfigPath);
            var config = deserializer.Deserialize<Config>(reader);
            return config;
        }
    }

    public class Config(string lethalCompanyVersion, Mod[] mods)
    {
        public readonly string lethalCompanyVersion = lethalCompanyVersion;
        public readonly Mod[] mods = mods;
    }

    public class Mod(string developer, string id, string version)
    {
        public readonly string developer = developer;
        public readonly string id = id;
        public readonly string version = version;
    }
}
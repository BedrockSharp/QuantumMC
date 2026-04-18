using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace QuantumMC.Config
{
    public static class ConfigManager
    {
        private const string ConfigPath = "Config.yml";

        public static ServerConfig Load()
        {
            // Generate default config if it doesn't exist
            if (!File.Exists(ConfigPath))
            {
                var defaults = new ServerConfig();
                Save(defaults);
                return defaults;
            }

            var yaml = File.ReadAllText(ConfigPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<ServerConfig>(yaml);
        }

        private static void Save(ServerConfig config)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(config);
            File.WriteAllText(ConfigPath, yaml);
        }
    }
}

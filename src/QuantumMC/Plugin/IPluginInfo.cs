namespace QuantumMC.Plugin
{
    public interface IPluginInfo
    {
        string Name { get; }
        string Version { get; }
        string Authors { get; }
        string Description { get; }
        string Api { get; }
    }

    internal class PluginInfo : IPluginInfo
    {
        public string Name { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public string Authors { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Api { get; init; } = string.Empty;

        public PluginInfo(PluginAttribute attribute)
        {
            Name = attribute.Name;
            Version = attribute.Version;
            Authors = attribute.Authors;
            Description = attribute.Description;
            Api = attribute.Api;
        }
    }
}

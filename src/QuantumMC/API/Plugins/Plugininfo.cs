namespace QuantumMC.API.Plugins
{
    /// <summary>
    /// Immutable snapshot of a plugin's metadata, populated from its
    /// <see cref="PluginAttribute"/> at load time.
    /// </summary>
    public sealed class PluginInfo
    {
        public string Name        { get; init; } = string.Empty;
        public string Version     { get; init; } = "1.0.0";
        public string Authors     { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string[] Dependencies { get; init; } = System.Array.Empty<string>();

        public override string ToString() =>
            $"{Name} v{Version} by {Authors}";

        public static PluginInfo FromAttribute(PluginAttribute attr) => new()
        {
            Name         = attr.Name,
            Version      = attr.Version,
            Authors      = attr.Authors,
            Description  = attr.Description,
            Dependencies = attr.Dependencies,
        };
    }
}

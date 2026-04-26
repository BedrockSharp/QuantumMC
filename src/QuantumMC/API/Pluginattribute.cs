using System;

namespace QuantumMC.API.Plugins
{
    /// <summary>
    /// Marks a class as a QuantumMC plugin and provides metadata about it.
    /// Apply this attribute to any class that extends <see cref="PluginBase"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginAttribute : Attribute
    {
        /// <summary>The display name of the plugin.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Semantic version string, e.g. "1.0.0".</summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>Comma-separated list of author names.</summary>
        public string Authors { get; set; } = string.Empty;

        /// <summary>Short human-readable description of what the plugin does.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional: list of plugin Names that must be loaded before this one.
        /// The loader will respect this order.
        /// </summary>
        public string[] Dependencies { get; set; } = Array.Empty<string>();
    }
}

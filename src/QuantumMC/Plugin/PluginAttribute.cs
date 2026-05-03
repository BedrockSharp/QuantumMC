using System;

namespace QuantumMC.Plugin
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Api { get; set; } = string.Empty;
        // Add More Attributes to the plugin api mechanic
    }
}

using System;

namespace QuantumMC.API.Plugins
{
    /// <summary>
    /// Marks a public property on a <see cref="PluginBase"/> subclass for
    /// automatic dependency injection by the plugin loader.
    ///
    /// <example>
    /// <code>
    /// [Inject] public ILogger Logger { get; set; }
    /// [Inject] public IEventBus Events { get; set; }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute { }
}

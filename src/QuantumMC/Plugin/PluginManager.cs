using System.Reflection;
using Serilog;

namespace QuantumMC.Plugin
{
    public class PluginManager
    {
        public Event.EventManager EventManager { get; }
        private readonly string _pluginsDirectory;
        private readonly List<PluginBase> _plugins = new();

        public PluginManager()
        {
            EventManager = new Event.EventManager();
            _pluginsDirectory = Path.Combine(QuantumMC.DataFolder, "plugins");

            if (!Directory.Exists(_pluginsDirectory))
            {
                Directory.CreateDirectory(_pluginsDirectory);
            }
        }

        public void Init()
        {
            LoadPluginsAsync().GetAwaiter().GetResult();
            EnablePluginsAsync().GetAwaiter().GetResult();
        }

        public async Task LoadPluginsAsync()
        {
            if (!Directory.Exists(_pluginsDirectory)) return;

            var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    await LoadPluginAsync(dllFile);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load plugin from {File}", dllFile);
                }
            }
        }

        private async Task LoadPluginAsync(string dllPath)
        {
            var loadContext = new PluginLoadContext(dllPath);
            var assembly = loadContext.LoadFromAssemblyPath(dllPath);

            var pluginTypes = assembly.GetExportedTypes()
                .Where(t => typeof(PluginBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var type in pluginTypes)
            {
                var attribute = type.GetCustomAttribute<PluginAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var plugin = (PluginBase?)Activator.CreateInstance(type);
                if (plugin != null)
                {
                    plugin.Info = new PluginInfo(attribute);
                    plugin.Logger = new PluginLogger(plugin.Info.Name);
                    _plugins.Add(plugin);
                    
                    Log.Information("Loaded plugin: {Name} v{Version}", plugin.Info.Name, plugin.Info.Version);
                }
            }
        }

        public async Task EnablePluginsAsync()
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    Log.Information("Enabling plugin: {Name}...", plugin.Info.Name);
                    await plugin.OnEnable();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error enabling plugin {Name}", plugin.Info.Name);
                }
            }
        }

        public async Task DisablePluginsAsync()
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    Log.Information("Disabling plugin: {Name}...", plugin.Info.Name);
                    await plugin.OnDisable();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error disabling plugin {Name}", plugin.Info.Name);
                }
            }
            _plugins.Clear();
        }
    }
}

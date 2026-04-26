using Serilog;

namespace QuantumMC.Plugin
{
    public class PluginLogger
    {
        private readonly string _pluginName;

        public PluginLogger(string pluginName)
        {
            _pluginName = pluginName;
        }

        public void Information(string message) => Log.Information("[{PluginName}] {Message}", _pluginName, message);
        public void Warning(string message) => Log.Warning("[{PluginName}] {Message}", _pluginName, message);
        public void Error(string message, Exception? ex = null) => Log.Error(ex, "[{PluginName}] {Message}", _pluginName, message);
        public void Debug(string message) => Log.Debug("[{PluginName}] {Message}", _pluginName, message);
    }
}

namespace QuantumMC.API.Plugins.Services
{
    public enum LogLevel { Debug, Info, Warn, Error, Fatal }

    /// <summary>
    /// Scoped logger automatically injected into plugins via [Inject].
    /// Each plugin gets its own logger instance prefixed with the plugin name.
    /// </summary>
    public interface ILogger
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        void Debug(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
    }
}

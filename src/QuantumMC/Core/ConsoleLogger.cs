using System;
using QuantumMC.API.Plugins.Services;

namespace QuantumMC.Core
{
    public sealed class ConsoleLogger : ILogger
    {
        private readonly string _prefix;

        public ConsoleLogger(string pluginName)
        {
            _prefix = $"[{pluginName}]";
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var (color, label) = level switch
            {
                LogLevel.Debug => (ConsoleColor.Gray,    "DEBUG"),
                LogLevel.Info  => (ConsoleColor.Cyan,    "INFO "),
                LogLevel.Warn  => (ConsoleColor.Yellow,  "WARN "),
                LogLevel.Error => (ConsoleColor.Red,     "ERROR"),
                LogLevel.Fatal => (ConsoleColor.Magenta, "FATAL"),
                _              => (ConsoleColor.White,   "INFO "),
            };

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] [{label}] {_prefix} {message}");
            Console.ResetColor();
        }

        public void Debug(string message) => Log(message, LogLevel.Debug);
        public void Warn(string message)  => Log(message, LogLevel.Warn);
        public void Error(string message) => Log(message, LogLevel.Error);
        public void Fatal(string message) => Log(message, LogLevel.Fatal);
    }
}

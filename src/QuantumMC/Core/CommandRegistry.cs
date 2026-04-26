using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using QuantumMC.API.Plugins.Services;

namespace QuantumMC.Core
{
    public sealed class CommandRegistry : ICommandRegistry
    {
        private record CommandEntry(string Name, string Description, Action<CommandContext> Handler);

        private readonly ConcurrentDictionary<string, CommandEntry> _commands = new(StringComparer.OrdinalIgnoreCase);

        public void Register(string name, string description, Action<CommandContext> handler)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(handler);
            _commands[name] = new CommandEntry(name, description, handler);
        }

        public void Unregister(string name)
        {
            _commands.TryRemove(name, out _);
        }

        /// <summary>
        /// Dispatch a raw command string (without leading slash).
        /// Returns true if a handler was found and invoked.
        /// </summary>
        public bool Dispatch(string rawInput, string? senderName = null)
        {
            var parts = rawInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            var cmdName = parts[0];
            if (!_commands.TryGetValue(cmdName, out var entry)) return false;

            var args = parts[1..];
            entry.Handler(new CommandContext
            {
                CommandName = cmdName,
                Args        = args,
                SenderName  = senderName,
            });
            return true;
        }

        public IReadOnlyDictionary<string, string> GetHelp()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, entry) in _commands)
                result[name] = entry.Description;
            return result;
        }
    }
}

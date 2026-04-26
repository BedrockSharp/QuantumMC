using System;
using System.Collections.Generic;
using QuantumMC.API.Plugins.Services;
using QuantumMC.API.World;

namespace QuantumMC.Core
{
    public sealed class QuantumServer : IServer
    {
        public string Name { get; }
        public IWorld DefaultWorld { get; }
        public IReadOnlyDictionary<string, IWorld> Worlds { get; }

        private readonly ILogger _logger;

        public QuantumServer(string name, ILogger logger)
        {
            Name    = name;
            _logger = logger;

            var overworld = new SimpleWorld("Overworld", "overworld");
            DefaultWorld  = overworld;
            Worlds        = new Dictionary<string, IWorld>
            {
                ["Overworld"] = overworld,
                ["Nether"]    = new SimpleWorld("Nether", "nether"),
                ["The End"]   = new SimpleWorld("The End", "end"),
            };
        }

        public void BroadcastMessage(string message)
        {
            _logger.Log($"[BROADCAST] {message}");
        }

        public void Shutdown(string reason = "Server shutting down")
        {
            _logger.Warn($"Server shutdown requested: {reason}");
        }

        // Minimal IWorld implementation for the built-in worlds
        private sealed class SimpleWorld : IWorld
        {
            public string Name       { get; }
            public string Dimension  { get; }
            public int    PlayerCount => 0;   // would be live in a real server

            public SimpleWorld(string name, string dimension)
            {
                Name      = name;
                Dimension = dimension;
            }
        }
    }
}

using System.Collections.Generic;
using QuantumMC.API.World;

namespace QuantumMC.API.Plugins.Services
{
    /// <summary>
    /// Represents the running QuantumMC server instance.
    /// Injected into plugin lifecycle methods.
    /// </summary>
    public interface IServer
    {
        /// <summary>Name/MOTD of the server.</summary>
        string Name { get; }

        /// <summary>The world players spawn into by default.</summary>
        IWorld DefaultWorld { get; }

        /// <summary>All loaded worlds, keyed by name.</summary>
        IReadOnlyDictionary<string, IWorld> Worlds { get; }

        /// <summary>Broadcast a message to all connected players.</summary>
        void BroadcastMessage(string message);

        /// <summary>Shutdown the server gracefully.</summary>
        void Shutdown(string reason = "Server shutting down");
    }
}

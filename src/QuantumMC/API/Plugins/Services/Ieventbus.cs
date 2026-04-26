using System;

namespace QuantumMC.API.Plugins.Services
{
    /// <summary>
    /// Simple publish/subscribe event bus injected into plugins via [Inject].
    /// Plugins use this to communicate without direct coupling.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>Subscribe to all events of type <typeparamref name="TEvent"/>.</summary>
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>Publish an event to all current subscribers.</summary>
        void Publish<TEvent>(TEvent @event) where TEvent : class;
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using QuantumMC.API.Plugins.Services;

namespace QuantumMC.Core
{
    public sealed class EventBus : IEventBus
    {
        // Map event type → list of (id, handler) pairs
        private readonly ConcurrentDictionary<Type, List<(Guid Id, Delegate Handler)>> _handlers = new();
        private readonly object _lock = new();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var id   = Guid.NewGuid();
            var type = typeof(TEvent);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(type, out var list))
                {
                    list = new List<(Guid, Delegate)>();
                    _handlers[type] = list;
                }
                list.Add((id, handler));
            }

            return new Subscription(() =>
            {
                lock (_lock)
                {
                    if (_handlers.TryGetValue(type, out var l))
                        l.RemoveAll(h => h.Id == id);
                }
            });
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            var type = typeof(TEvent);
            List<(Guid, Delegate)>? snapshot = null;

            lock (_lock)
            {
                if (_handlers.TryGetValue(type, out var list))
                    snapshot = new List<(Guid, Delegate)>(list);
            }

            if (snapshot is null) return;
            foreach (var (_, handler) in snapshot)
                ((Action<TEvent>)handler)(@event);
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _unsubscribe();
            }
        }
    }
}

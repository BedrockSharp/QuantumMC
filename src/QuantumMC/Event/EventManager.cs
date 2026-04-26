using System.Reflection;
using Serilog;

namespace QuantumMC.Event
{
    public class EventManager
    {
        private readonly Dictionary<Type, List<RegisteredHandler>> _handlers = new();

        public void RegisterListeners(Listener listener)
        {
            var type = listener.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<EventHandlerAttribute>();
                if (attribute == null) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1 || !typeof(Event).IsAssignableFrom(parameters[0].ParameterType))
                {
                    Log.Warning("Method {Method} in {Type} has [EventHandler] but invalid parameters.", method.Name, type.Name);
                    continue;
                }

                var eventType = parameters[0].ParameterType;
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<RegisteredHandler>();
                }

                _handlers[eventType].Add(new RegisteredHandler(listener, method, attribute.Priority));
                
                _handlers[eventType] = _handlers[eventType]
                    .OrderBy(h => h.Priority)
                    .ToList();
            }
        }

        public async Task CallEventAsync<T>(T @event) where T : Event
        {
            if (!_handlers.TryGetValue(typeof(T), out var handlers)) return;

            foreach (var handler in handlers)
            {
                try
                {
                    var result = handler.Method.Invoke(handler.Listener, new object[] { @event });
                    if (result is Task task)
                    {
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while calling event {Event} in handler {Method}", typeof(T).Name, handler.Method.Name);
                }
            }
        }

        private record RegisteredHandler(Listener Listener, MethodInfo Method, EventPriority Priority);
    }
}

using System;

namespace QuantumMC.Event
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        public EventPriority Priority { get; set; } = EventPriority.Normal;
    }

    public enum EventPriority
    {
        Lowest,
        Low,
        Normal,
        High,
        Highest,
        Monitor
    }
}

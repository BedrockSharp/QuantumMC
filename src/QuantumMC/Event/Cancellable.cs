namespace QuantumMC.Event
{
    public interface Cancellable
    {
        bool IsCancelled { get; set; }
    }
}

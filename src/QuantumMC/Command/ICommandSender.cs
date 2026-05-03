namespace QuantumMC.Command
{
    public interface ICommandSender
    {
        void SendMessage(string message);
        Server GetServer();
        string GetName();
        bool HasPermission(string permission);
    }
}

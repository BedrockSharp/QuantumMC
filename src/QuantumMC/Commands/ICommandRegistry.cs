namespace QuantumMC.Commands
{
    public interface ICommandRegistry
    {
        void RegisterCommands(Action<string, CommandExecutor> register);
    }
}
namespace QuantumMC.Command
{
    public interface ICommandMap
    {
        void Register(string fallbackPrefix, Command command);
        void RegisterAll(string fallbackPrefix, List<Command> commands);
        bool Dispatch(ICommandSender sender, string commandLine);
        void ClearCommands();
        Command? GetCommand(string name);
    }
}

namespace QuantumMC.Commands
{
    public abstract class CommandExecutor
    {
        public abstract bool OnCommand(ICommandSender sender, Command command, string label, string[] args);
    }
}